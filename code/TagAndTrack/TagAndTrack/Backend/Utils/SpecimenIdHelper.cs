namespace TagAndTrack.Backend.Utils
{
    public static class SpecimenIdHelper
    {
        /// <summary>
        /// Returns true if the comma-separated list of specimen IDs contains the given ID.
        /// Handles null/empty strings, partial numeric matches, and whitespace.
        /// </summary>
        public static bool ContainsSpecimenId(string? specimenIdsCsv, int specimenId)
        {
            if (string.IsNullOrEmpty(specimenIdsCsv))
                return false;

            var ids = specimenIdsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries);
            return ids.Any(idStr => int.TryParse(idStr.Trim(), out int id) && id == specimenId);
        }
    }
}
