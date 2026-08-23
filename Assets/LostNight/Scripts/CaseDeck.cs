using System;
using System.Collections.Generic;

namespace LostNight
{
    public sealed class CaseDeck
    {
        private readonly List<int> remainingIndices = new();
        private readonly Random random = new();
        private IReadOnlyList<LostItemCaseDefinition> catalog;

        public void Reset(IReadOnlyList<LostItemCaseDefinition> source)
        {
            catalog = source;
            remainingIndices.Clear();
            for (var i = 0; i < source.Count; i++) remainingIndices.Add(i);
            for (var i = remainingIndices.Count - 1; i > 0; i--)
            {
                var swapIndex = random.Next(i + 1);
                (remainingIndices[i], remainingIndices[swapIndex]) = (remainingIndices[swapIndex], remainingIndices[i]);
            }
        }

        public LostItemCaseDefinition Draw()
        {
            if (catalog == null || remainingIndices.Count == 0)
                throw new InvalidOperationException("出題可能な未使用案件がありません。");
            var last = remainingIndices.Count - 1;
            var result = catalog[remainingIndices[last]];
            remainingIndices.RemoveAt(last);
            return result;
        }
    }
}
