﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConferenceDTO
{
    public class SearchResult
    {
        public SearchResultType Type { get; set; }

        public JObject Value { get; set; }
    }

    public enum SearchResultType
    {
        Attendee,
        Conference,
        Session,
        Track,
        Speaker
    }
}
