﻿using System.Collections.Generic;
using BookPortal.Web.Domain.Models;

namespace BookPortal.Web.Models
{
    public class TranslationResponse
    {
        public int TranslationWorkId { get; set; }

        public List<PersonResponse> Translators { get; set; }

        public int? WorkId { get; set; }

        public string WorkName { get; set; }

        public int? WorkYear { get; set; }

        public List<PersonResponse> Authors { get; set; }

        public int? TranslationYear { get; set; }

        public string WorkTypeName { get; set; }

        public string WorkTypeNameSingle { get; set; }

        public int? WorkTypeLevel { get; set; }

        public List<string> Names { get; set; }

        public List<int> Editions { get; set; }

        public int? LanguageId { get; set; }
    }
}
