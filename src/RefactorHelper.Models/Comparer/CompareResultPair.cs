﻿using RefactorHelper.Models.External;

namespace RefactorHelper.Models.Comparer
{
    public class CompareResultPair
    {
        public required List<Diff> Diffs { get; init; }

        public required CompareResult Result1 { get; init; }

        public required CompareResult Result2 { get; init; }
    }
}
