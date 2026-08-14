namespace WeatherApp.Core.Translator;

public static class RegionTranslator
{
    private static readonly Dictionary<string, string> _translations = new(StringComparer.OrdinalIgnoreCase)
    {
        // Города федерального значения
        { "Moscow", "Московская область" },
        { "Moskva", "Московская область" },
        { "Moscow City", "Московская область" },
        { "Saint Petersburg", "Санкт-Петербург" },
        { "Saint Petersburg City", "Санкт-Петербург" },
        { "Sevastopol", "Севастополь" },
        { "Sevastopol City", "Севастополь" },

        // Области (ключи - названия регионов, которые приходят от API)
        { "Krasnodar", "Краснодарский край" },
        { "Krasnodar Krai", "Краснодарский край" },
        { "Sverdlovsk", "Свердловская область" },
        { "Sverdlovsk Oblast", "Свердловская область" },
        { "Novosibirsk", "Новосибирская область" },
        { "Novosibirsk Oblast", "Новосибирская область" },
        { "Nizhny Novgorod", "Нижегородская область" },
        { "Nizhny Novgorod Oblast", "Нижегородская область" },
        { "Chelyabinsk", "Челябинская область" },
        { "Chelyabinsk Oblast", "Челябинская область" },
        { "Omsk", "Омская область" },
        { "Omsk Oblast", "Омская область" },
        { "Samara", "Самарская область" },
        { "Samara Oblast", "Самарская область" },
        { "Rostov", "Ростовская область" },
        { "Rostov Oblast", "Ростовская область" },
        { "Krasnoyarsk", "Красноярский край" },
        { "Krasnoyarsk Krai", "Красноярский край" },
        { "Perm", "Пермский край" },
        { "Perm Krai", "Пермский край" },
        { "Voronezh", "Воронежская область" },
        { "Voronezh Oblast", "Воронежская область" },
        { "Volgograd", "Волгоградская область" },
        { "Volgograd Oblast", "Волгоградская область" },
        { "Saratov", "Саратовская область" },
        { "Saratov Oblast", "Саратовская область" },
        { "Tyumen", "Тюменская область" },
        { "Tyumen Oblast", "Тюменская область" },
        { "Altai", "Алтайский край" },
        { "Altai Krai", "Алтайский край" },
        { "Primorsky", "Приморский край" },
        { "Primorsky Krai", "Приморский край" },
        { "Khabarovsk", "Хабаровский край" },
        { "Khabarovsk Krai", "Хабаровский край" },
        { "Irkutsk", "Иркутская область" },
        { "Irkutsk Oblast", "Иркутская область" },
        { "Kemerovo", "Кемеровская область" },
        { "Kemerovo Oblast", "Кемеровская область" },
        { "Orenburg", "Оренбургская область" },
        { "Orenburg Oblast", "Оренбургская область" },
        { "Ryazan", "Рязанская область" },
        { "Ryazan Oblast", "Рязанская область" },
        { "Tula", "Тульская область" },
        { "Tula Oblast", "Тульская область" },
        { "Kirov", "Кировская область" },
        { "Kirov Oblast", "Кировская область" },
        { "Kaliningrad", "Калининградская область" },
        { "Kaliningrad Oblast", "Калининградская область" },
        { "Astrakhan", "Астраханская область" },
        { "Astrakhan Oblast", "Астраханская область" },
        { "Penza", "Пензенская область" },
        { "Penza Oblast", "Пензенская область" },
        { "Lipetsk", "Липецкая область" },
        { "Lipetsk Oblast", "Липецкая область" },
        { "Kursk", "Курская область" },
        { "Kursk Oblast", "Курская область" },
        { "Belgorod", "Белгородская область" },
        { "Belgorod Oblast", "Белгородская область" },
        { "Kaluga", "Калужская область" },
        { "Kaluga Oblast", "Калужская область" },
        { "Oryol", "Орловская область" },
        { "Oryol Oblast", "Орловская область" },
        { "Vladimir", "Владимирская область" },
        { "Vladimir Oblast", "Владимирская область" },
        { "Yaroslavl", "Ярославская область" },
        { "Yaroslavl Oblast", "Ярославская область" },
        { "Tver", "Тверская область" },
        { "Tver Oblast", "Тверская область" },
        { "Smolensk", "Смоленская область" },
        { "Smolensk Oblast", "Смоленская область" },
        { "Bryansk", "Брянская область" },
        { "Bryansk Oblast", "Брянская область" },
        { "Pskov", "Псковская область" },
        { "Pskov Oblast", "Псковская область" },
        { "Novgorod", "Новгородская область" },
        { "Novgorod Oblast", "Новгородская область" },
        { "Murmansk", "Мурманская область" },
        { "Murmansk Oblast", "Мурманская область" },
        { "Arkhangelsk", "Архангельская область" },
        { "Arkhangelsk Oblast", "Архангельская область" },
        { "Vologda", "Вологодская область" },
        { "Vologda Oblast", "Вологодская область" },
        { "Kostroma", "Костромская область" },      // <-- ТОЛЬКО ОДИН РАЗ!
        { "Kostroma Oblast", "Костромская область" },
        { "Ivanovo", "Ивановская область" },
        { "Ivanovo Oblast", "Ивановская область" },
        { "Tambov", "Тамбовская область" },
        { "Tambov Oblast", "Тамбовская область" },
        { "Kurgan", "Курганская область" },
        { "Kurgan Oblast", "Курганская область" },
        { "Magadan", "Магаданская область" },
        { "Magadan Oblast", "Магаданская область" },
        { "Amur", "Амурская область" },
        { "Amur Oblast", "Амурская область" },
        { "Sakhalin", "Сахалинская область" },
        { "Sakhalin Oblast", "Сахалинская область" },
        { "Ulyanovsk", "Ульяновская область" },
        { "Ulyanovsk Oblast", "Ульяновская область" },
        { "Tomsk", "Томская область" },
        { "Tomsk Oblast", "Томская область" },

        // Республики
        { "Tatarstan", "Республика Татарстан" },
        { "Bashkortostan", "Республика Башкортостан" },
        { "Udmurtia", "Удмуртская Республика" },
        { "Chuvashia", "Чувашская Республика" },
        { "Mari El", "Республика Марий Эл" },
        { "Crimea", "Республика Крым" },
        { "Republic of Crimea", "Республика Крым" },
        { "Chechnya", "Чеченская Республика" },
        { "Dagestan", "Республика Дагестан" },
        { "Kabardino-Balkaria", "Кабардино-Балкарская Республика" },
        { "Karachay-Cherkessia", "Карачаево-Черкесская Республика" },
        { "North Ossetia", "Республика Северная Осетия - Алания" },
        { "Ingushetia", "Республика Ингушетия" },
        { "Kalmykia", "Республика Калмыкия" },
        { "Adygea", "Республика Адыгея" },
        { "Karelia", "Республика Карелия" },
        { "Komi", "Республика Коми" },
        { "Komi Republic", "Республика Коми" },
        { "Sakha", "Республика Саха (Якутия)" },
        { "Sakha Republic", "Республика Саха (Якутия)" },
        { "Buryatia", "Республика Бурятия" },
        { "Tuva", "Республика Тыва" },
        { "Khakassia", "Республика Хакасия" },
        { "Altai Republic", "Республика Алтай" },

        // Края (дополнительно)
        { "Kamchatka Krai", "Камчатский край" },
        { "Zabaykalsky Krai", "Забайкальский край" },
        { "Stavropol Krai", "Ставропольский край" },

        // Автономные округа
        { "Nenets", "Ненецкий автономный округ" },
        { "Nenets Autonomous Okrug", "Ненецкий автономный округ" },
        { "Khanty-Mansi", "Ханты-Мансийский автономный округ" },
        { "Khanty-Mansi Autonomous Okrug", "Ханты-Мансийский автономный округ" },
        { "Chukotka", "Чукотский автономный округ" },
        { "Chukotka Autonomous Okrug", "Чукотский автономный округ" },
        { "Yamalo-Nenets", "Ямало-Ненецкий автономный округ" },
        { "Yamalo-Nenets Autonomous Okrug", "Ямало-Ненецкий автономный округ" },

        // Еврейская автономная область
        { "Jewish Autonomous Oblast", "Еврейская автономная область" },
        { "Jewish Autonomous", "Еврейская автономная область" },
    };

    public static string Translate(string regionName)
    {
        if (string.IsNullOrWhiteSpace(regionName))
            return regionName ?? string.Empty;

        // Прямое совпадение
        if (_translations.TryGetValue(regionName, out var translated))
            return translated;

        // Частичное совпадение
        foreach (var kvp in _translations)
        {
            if (regionName.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                return kvp.Value;
        }

        return regionName;
    }
}