namespace SGC_PORTFOLIO.Models.Dtos
{
    public class NewsletterRegistrationDto
    {
        /// <summary>
        /// The UserId of the user who is registering. 
        /// (Generated at sign-up or lookup time.)
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Email address (must match a registered user or be used to create one).
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Optional age tag (e.g., "25", "30", etc.). 
        /// If supplied, it will appear later in clustering.
        /// </summary>
        public string? AgeTag { get; set; }

        /// <summary>
        /// Optional field of work (e.g. "Software", "Marketing", etc.).
        /// </summary>
        public string? FieldOfWork { get; set; }

        /// <summary>
        /// Optional experience tag (e.g. "Junior", "Senior", etc.).
        /// </summary>
        public string? ExperienceTag { get; set; }

        /// <summary>
        /// Up to 12 topic/interest tags (must be among the system’s 200–300 predefined tags).
        /// </summary>
        public List<string> TopicTags { get; set; }
    }
}
