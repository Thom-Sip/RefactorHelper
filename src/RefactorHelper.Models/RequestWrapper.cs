﻿using RefactorHelper.Models.Comparer;
using RefactorHelper.Models.RequestHandler;
using RefactorHelper.Models.SwaggerProcessor;

namespace RefactorHelper.Models
{
    public class RequestWrapper
    {
        public required int ID { get; set; }

        public required RequestDetails Request { get; set; }

        public RefactorTestResultPair? TestResult { get; set; }

        public CompareResultPair? CompareResultPair { get; set; }

        public string ResultHtml { get; set; } = string.Empty;
    }
}