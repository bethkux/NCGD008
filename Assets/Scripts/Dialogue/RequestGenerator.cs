namespace VerdantBrews
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Localization.Settings;

    /// <summary>
    /// Generates customer drink requests based on randomly selected ingredient combinations.
    /// The request is derived from the resulting qualities of three random ingredients,
    /// ensuring that every generated request is always solvable by the player.
    /// </summary>
    public static class RequestGenerator
    {
        /// <summary>
        /// The currently active request qualities.
        /// Used by gameplay systems to validate the player's drink.
        /// </summary>
        public static List<(QualityType, QualityLevel)> CurrentRequest = new();

        private static bool first = true;
        private static int successfulRequests = 0;

        /// <summary>
        /// List of available ingredients.
        /// Must be assigned externally at runtime (e.g. from UI bindings or ingredient database).
        /// </summary>
        public static List<IngredientData> Ingredients = new();

        /// <summary>
        /// Registers a successfully completed request.
        /// Used to increase request difficulty over time.
        /// </summary>
        public static void RegisterSuccess()
        {
            successfulRequests++;
        }

        /// <summary>
        /// Generates a new customer request.
        /// The number of requested qualities increases with the number of successful drinks.
        /// </summary>
        public static string GenerateRandomCustomerRequest()
        {
            // First request has no quality requirements
            if (first)
            {
                first = false;
                return GenerateCustomerRequest(0);
            }

            // Gradually increase number of requested qualities (1–4)
            int qualities = Mathf.Clamp((successfulRequests / 5) + 1, 1, 4);

            return GenerateCustomerRequest(qualities);
        }

        /// <summary>
        /// Generates a request containing a specific number of qualities.
        /// </summary>
        public static string GenerateCustomerRequest(int count)
        {
            // Generate qualities derived from a random ingredient combination
            var qualities = GenerateRequestFromIngredients(count);

            var parts = new List<string>();

            // Convert each quality to a localized text fragment
            foreach (var (type, level) in qualities)
            {
                string key = GetQualityKey(type, level);
                parts.Add(GetText("Qualities", key));
            }

            // Select a dialogue template based on number of requested qualities
            string templateKey = qualities.Count switch
            {
                0 => "request_none",
                1 => "request_single",
                2 => "request_double",
                3 => "request_triple",
                _ => "request_quadruple"
            };

            // Build the final localized sentence
            var entry = GetText("DialogueTemplate", "Dialogue." + templateKey, parts.ToArray());
            var greeting = GetText("DialogueTemplate", "Dialogue.greeting" + Random.Range(1, 3));

            return greeting + " " + entry;
        }

        /// <summary>
        /// Creates a set of requested qualities by randomly selecting three ingredients
        /// and converting their combined stats into quality levels.
        /// </summary>
        private static List<(QualityType, QualityLevel)> GenerateRequestFromIngredients(int count)
        {
            // Pick three random ingredients
            var a = Ingredients[Random.Range(0, Ingredients.Count)];
            var b = Ingredients[Random.Range(0, Ingredients.Count)];
            var c = Ingredients[Random.Range(0, Ingredients.Count)];

            // Sum their qualities
            int warmth = a.Warmth + b.Warmth + c.Warmth;
            int relaxation = a.Relaxation + b.Relaxation + c.Relaxation;
            int sharpness = a.Sharpness + b.Sharpness + c.Sharpness;
            int density = a.Heaviness + b.Heaviness + c.Heaviness;

            // Convert raw values to qualitative levels
            var all = new List<(QualityType, QualityLevel)>
            {
                (QualityType.Warmth, ConvertToLevel(warmth)),
                (QualityType.Relaxation, ConvertToLevel(relaxation)),
                (QualityType.Sharpness, ConvertToLevel(sharpness)),
                (QualityType.Density, ConvertToLevel(density))
            };

            var result = new List<(QualityType, QualityLevel)>();

            // Randomly select which qualities will be requested
            for (int i = 0; i < count && all.Count > 0; i++)
            {
                int index = Random.Range(0, all.Count);
                result.Add(all[index]);
                all.RemoveAt(index);
            }

            CurrentRequest = result;
            return result;
        }

        /// <summary>
        /// Converts a raw stat value into a quality intensity level.
        /// </summary>
        private static QualityLevel ConvertToLevel(int value)
        {
            if (value <= 1) return QualityLevel.Low;
            if (value <= 3) return QualityLevel.Medium;
            return QualityLevel.High;
        }

        /// <summary>
        /// Builds the localization key for a specific quality and level.
        /// Example: "Qualities.warmth_high"
        /// </summary>
        private static string GetQualityKey(QualityType type, QualityLevel level)
        {
            return $"Qualities.{type.ToString().ToLower()}_{level.ToString().ToLower()}";
        }

        /// <summary>
        /// Retrieves a localized string from the specified table.
        /// Optional arguments are used for template replacements.
        /// </summary>
        private static string GetText(string table, string key, string[] array = null)
        {
            var entry = LocalizationSettings.StringDatabase.GetTable(table).GetEntry(key);
            return entry.GetLocalizedString(array);
        }

        /// <summary>
        /// Resets the request generator state.
        /// Used when starting a new game/session.
        /// </summary>
        public static void Reset()
        {
            CurrentRequest.Clear();
            first = true;
            successfulRequests = 0;
        }
    }

    /// <summary>
    /// Types of drink qualities a customer may request.
    /// </summary>
    public enum QualityType
    {
        Warmth,
        Relaxation,
        Sharpness,
        Density
    }

    /// <summary>
    /// Intensity levels for drink qualities.
    /// </summary>
    public enum QualityLevel
    {
        Low,
        Medium,
        High
    }
}