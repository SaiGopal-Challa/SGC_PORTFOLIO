namespace SGC_PORTFOLIO.Models.Dtos
{
    public class ModifyInterestsDto
    {
        /// <summary>
        /// The UserId for whom to modify interest tags.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// New list of up to 12 topic/interest tags.
        /// </summary>
        public List<string> TopicTags { get; set; }
    }
}
