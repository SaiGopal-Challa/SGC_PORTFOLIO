public class ClusterInfoDetail
{
    public int ClusterId { get; set; }
    public DateTime ClusterCreatedAtUtc { get; set; }
    public string RepresentativeTags { get; set; }
    public List<string> GetRepresentativeTagList()
    {
        if (string.IsNullOrWhiteSpace(RepresentativeTags))
            return new List<string>();
        return new List<string>(RepresentativeTags.Split(';', StringSplitOptions.RemoveEmptyEntries));
    }

    public string AgeTag { get; set; } // Most common age tag in cluster
    public string FieldOfWork { get; set; } // Most common field of work in cluster
    public string ExperienceTag { get; set; } // Most common experience tag in cluster
}
