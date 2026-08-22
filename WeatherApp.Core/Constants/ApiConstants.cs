namespace WeatherApp.Core.Constants;

public static class ApiConstants
{
    // Базовые URL
    public const string WeatherApiBaseUrl = "https://api.weatherapi.com/v1/";
    public const string NominatimBaseUrl = "https://nominatim.openstreetmap.org/";

    // Эндпоинты
    public const string CurrentWeatherEndpoint = "current.json";
    public const string ForecastEndpoint = "forecast.json";
    public const string SearchEndpoint = "search.json";

    // Параметры Nominatim
    public const string NominatimFormat = "json";
    public const string NominatimUserAgent = "WeatherApp/1.0";

    // Параметры по умолчанию
    public const int DefaultForecastDays = 5;
    public const int CacheDurationMinutes = 30;

    // Язык API (по умолчанию русский)
    public const string DefaultLanguage = "ru";

    // Доступные языки для WeatherAPI
    public static class Languages
    {
        public const string Arabic = "ar";
        public const string Bengali = "bn";
        public const string Bulgarian = "bg";
        public const string ChineseSimplified = "zh";
        public const string ChineseTraditional = "zh_tw";
        public const string Czech = "cs";
        public const string Danish = "da";
        public const string Dutch = "nl";
        public const string Finnish = "fi";
        public const string French = "fr";
        public const string German = "de";
        public const string Greek = "el";
        public const string Hindi = "hi";
        public const string Hungarian = "hu";
        public const string Italian = "it";
        public const string Japanese = "ja";
        public const string Javanese = "jv";
        public const string Korean = "ko";
        public const string Mandarin = "zh_cmn";
        public const string Marathi = "mr";
        public const string Polish = "pl";
        public const string Portuguese = "pt";
        public const string Punjabi = "pa";
        public const string Romanian = "ro";
        public const string Russian = "ru";
        public const string Serbian = "sr";
        public const string Sinhalese = "si";
        public const string Slovak = "sk";
        public const string Spanish = "es";
        public const string Swedish = "sv";
        public const string Tamil = "ta";
        public const string Telugu = "te";
        public const string Turkish = "tr";
        public const string Ukrainian = "uk";
        public const string Urdu = "ur";
        public const string Vietnamese = "vi";
        public const string WuShanghainese = "zh_wuu";
        public const string Xiang = "zh_hsn";
        public const string YueCantonese = "zh_yue";
        public const string Zulu = "zu";
    }
}