using Microsoft.ML;
using Microsoft.ML.Transforms;
using Microsoft.ML.Data;
using SGC_PORTFOLIO.Models.Entities;
using SGC_PORTFOLIO.Services.CsvHelpers;
using System.IO;
using Newtonsoft.Json;

namespace SGC_PORTFOLIO.Services
{
    public class ClusterService
    {
        private readonly string _userNewsletterCsvPath;
        private readonly string _clusterInfoCsvPath;
        private readonly int _clusterCount;
        private readonly MLContext _mlContext;

        public ClusterService(IConfiguration config)
        {
            var basePath = GetProjectRoot();
            _userNewsletterCsvPath = Path.Combine(basePath, "App_Data", "CSV", "UserNewsletter.csv");
            _clusterInfoCsvPath = Path.Combine(basePath, "App_Data", "CSV", "ClusterInfo.csv");

            _clusterCount = config.GetValue<int?>("Clustering:ClusterCount") ?? 2; // as per your requirement
            _mlContext = new MLContext(seed: 1);
        }

        private string GetProjectRoot()
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dir = new DirectoryInfo(baseDir);
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "SGC_PORTFOLIO.csproj")))
            {
                dir = dir.Parent;
            }
            return dir?.FullName ?? baseDir;
        }

        private class FeatureInput
        {
            public float[] Features { get; set; }
            public string UserIndex { get; set; }
        }

        private class ClusterPrediction
        {
            [ColumnName("PredictedLabel")]
            public uint PredictedClusterId { get; set; }
        }

        public async Task RunClusteringAsync()
        {
            // Load global tags from globalTags.json
            var basePath = GetProjectRoot();
            var globalTagsPath = Path.Combine(basePath, "globalTags.json");
            var globalTags = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(globalTagsPath));
            int globalTagCount = globalTags.Count; // e.g. 200
            int otherTagCount = 3;
            int featureCount = globalTagCount + otherTagCount;

            var users = CsvReaderUtility.ReadAll<UserNewsletterDetail>(_userNewsletterCsvPath);
            if (users == null || users.Count == 0)
                return;

            // Cross-check: ensure all users have exactly 15 tags
            foreach (var u in users)
            {
                var topicTags = u.GetTopicTagList(15);
                if (topicTags.Count != 15)
                {
                    Console.WriteLine($"[ClusterService] User {u.UserId} does not have 15 topic tags. Found: {topicTags.Count}");
                    return;
                }
                if (string.IsNullOrWhiteSpace(u.AgeTag) || string.IsNullOrWhiteSpace(u.FieldOfWork) || string.IsNullOrWhiteSpace(u.ExperienceTag))
                {
                    Console.WriteLine($"[ClusterService] User {u.UserId} missing one of the 3 required tags (AgeTag, FieldOfWork, ExperienceTag)");
                    return;
                }
            }

            // Build tag list: globalTags + 3 other tags
            var tagList = new List<string>(globalTags);
            tagList.Add("AgeTag");
            tagList.Add("FieldOfWork");
            tagList.Add("ExperienceTag");
            var tagToIndex = tagList.Select((tag, idx) => new { tag, idx }).ToDictionary(x => x.tag, x => x.idx, StringComparer.OrdinalIgnoreCase);

            // Build feature data for all users
            var featureData = users.Select(u =>
            {
                var features = new float[featureCount];
                var topicTagList = u.GetTopicTagList(15);
                foreach (var tag in topicTagList)
                {
                    if (!string.IsNullOrWhiteSpace(tag) && tagToIndex.TryGetValue(tag.Trim(), out var idx))
                        features[idx] = 1f;
                }
                if (!string.IsNullOrWhiteSpace(u.AgeTag) && tagToIndex.TryGetValue("AgeTag", out var idxAge))
                    features[idxAge] = 1f;
                if (!string.IsNullOrWhiteSpace(u.FieldOfWork) && tagToIndex.TryGetValue("FieldOfWork", out var idxField))
                    features[idxField] = 1f;
                if (!string.IsNullOrWhiteSpace(u.ExperienceTag) && tagToIndex.TryGetValue("ExperienceTag", out var idxExp))
                    features[idxExp] = 1f;
                return new FeatureInput { Features = features, UserIndex = u.UserId };
            }).ToList();

            // Explicitly set vector size in schema
            var schemaDef = SchemaDefinition.Create(typeof(FeatureInput));
            schemaDef["Features"].ColumnType = new VectorDataViewType(NumberDataViewType.Single, featureCount);
            var dataView = _mlContext.Data.LoadFromEnumerable(featureData, schemaDef);

            // 3. KMeans
            ITransformer model = null;
            try
            {
                var pipeline = _mlContext.Clustering.Trainers.KMeans(
                    featureColumnName: nameof(FeatureInput.Features),
                    numberOfClusters: _clusterCount);
                model = pipeline.Fit(dataView);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClusterService] Exception during Fit: {ex}");
                return;
            }

            // Create prediction engine after schema is set
            PredictionEngine<FeatureInput, ClusterPrediction> predictor;
            try
            {
                predictor = _mlContext.Model.CreatePredictionEngine<FeatureInput, ClusterPrediction>(model, inputSchemaDefinition: schemaDef);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClusterService] Exception during Create Prediction: {ex}");
                return;
            }

            // 4. Predict and assign cluster IDs
            var userClusters = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in featureData)
            {
                try
                {
                    var pred = predictor.Predict(item);
                    userClusters[item.UserIndex] = (int)pred.PredictedClusterId;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ClusterService] Exception during Predict for user {item.UserIndex}: {ex}");
                }
            }

            // 5. Update users’ ClusterId
            foreach (var u in users)
            {
                if (userClusters.TryGetValue(u.UserId, out var cid))
                    u.ClusterId = cid;
            }

            CsvWriterUtility.WriteAll(_userNewsletterCsvPath, users);

            // 6. Compute representative tags per cluster
            var clusterTagCounts = new Dictionary<int, Dictionary<string, int>>();
            for (int c = 1; c <= _clusterCount; c++)
                clusterTagCounts[c] = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var u in users)
            {
                if (!clusterTagCounts.ContainsKey(u.ClusterId)) continue;
                var dict = clusterTagCounts[u.ClusterId];

                void IncTag(string t)
                {
                    if (string.IsNullOrWhiteSpace(t)) return;
                    var key = t.Trim();
                    if (!dict.ContainsKey(key)) dict[key] = 0;
                    dict[key]++;
                }

                var topicTagList = u.GetTopicTagList(15);
                foreach (var tag in topicTagList)
                {
                    IncTag(tag);
                }
                IncTag(u.AgeTag);
                IncTag(u.FieldOfWork);
                IncTag(u.ExperienceTag);
            }

            var clusterInfos = new List<ClusterInfoDetail>();
            var nowUtc = DateTime.UtcNow;
            foreach (var kvp in clusterTagCounts)
            {
                int clusterId = kvp.Key;
                var topTags = kvp.Value
                    .OrderByDescending(t => t.Value)
                    .Where(t => t.Key != null && t.Key != "" && t.Key != "AgeTag" && t.Key != "FieldOfWork" && t.Key != "ExperienceTag")
                    .Take(7)
                    .Select(t => t.Key)
                    .ToList();
                var repTagsJoined = string.Join(';', topTags);

                // Get most common AgeTag, FieldOfWork, ExperienceTag, fallback to "Unknown" if missing
                string mostCommonAgeTag = kvp.Value
                    .Where(t => t.Key == "AgeTag")
                    .OrderByDescending(t => t.Value)
                    .Select(t => t.Key)
                    .FirstOrDefault();
                if (string.IsNullOrWhiteSpace(mostCommonAgeTag)) mostCommonAgeTag = "Unknown";

                string mostCommonFieldOfWork = kvp.Value
                    .Where(t => t.Key == "FieldOfWork")
                    .OrderByDescending(t => t.Value)
                    .Select(t => t.Key)
                    .FirstOrDefault();
                if (string.IsNullOrWhiteSpace(mostCommonFieldOfWork)) mostCommonFieldOfWork = "Unknown";

                string mostCommonExperienceTag = kvp.Value
                    .Where(t => t.Key == "ExperienceTag")
                    .OrderByDescending(t => t.Value)
                    .Select(t => t.Key)
                    .FirstOrDefault();
                if (string.IsNullOrWhiteSpace(mostCommonExperienceTag)) mostCommonExperienceTag = "Unknown";

                clusterInfos.Add(new ClusterInfoDetail
                {
                    ClusterId = clusterId,
                    ClusterCreatedAtUtc = nowUtc,
                    RepresentativeTags = repTagsJoined,
                    AgeTag = mostCommonAgeTag,
                    FieldOfWork = mostCommonFieldOfWork,
                    ExperienceTag = mostCommonExperienceTag
                });
            }

            CsvWriterUtility.WriteAll(_clusterInfoCsvPath, clusterInfos);
        }
    }
}
