using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PocketGems.Parameters.Editor.LocalCSV.Rows;

namespace PocketGems.Parameters.Editor.LocalCSV
{
    public class CSVUtilTest
    {
        private void AssertIdentifierSorted(string[] input, string[] output)
        {
            Assert.AreEqual(input.Length, output.Length);
            var rowDatas = new List<CSVRowData>();
            for (int i = 0; i < input.Length; i++)
                rowDatas.Add(new CSVRowData(null, new[] { input[i] }, ""));

            var outputRows = CSVUtil.IdentifierSort(rowDatas).ToList();
            Assert.AreEqual(output.Length, outputRows.Count);
            for (int i = 0; i < output.Length; i++)
                Assert.AreEqual(output[i], outputRows[i].Identifier);
        }

        /*
         * Model the sorting test cases based on how unity behaves with it's sorting
         */

        [Test]
        public void IdentifierSortNoOp()
        {
            AssertIdentifierSorted(
                new[] { "1", "2", "3" },
                new[] { "1", "2", "3" });
        }

        [Test]
        public void IdentifierSortBasic()
        {
            AssertIdentifierSorted(
                new[] { "3", "2", "1" },
                new[] { "1", "2", "3" });

            AssertIdentifierSorted(
                new[] { "AA", "AA", "A", "AAA" },
                new[] { "A", "AA", "AA", "AAA" });
        }

        [Test]
        public void IdentifierSortWordsNumbers()
        {
            AssertIdentifierSorted(
                new[] { "ABCD", "3", "2", "1", "ABC", "10", "C", "b" },
                new[] { "1", "2", "3", "10", "ABC", "ABCD", "b", "C" });
        }

        [Test]
        public void IdentifierSortPrefixNumbers()
        {
            AssertIdentifierSorted(
                new[] { "1A", "1b", "01", "1", "10", "10A", "2000", "a", "0", "00", "00" },
                new[] { "00", "00", "0", "01", "1", "1A", "1b", "10", "10A", "2000", "a" });
        }

        [Test]
        public void IdentifierSortSuffixNumbers()
        {
            AssertIdentifierSorted(
                new[] { "Table550", "Table5", "Table01", "Table02", "Table001", "Table0100", "Tables1" },
                new[] { "Table001", "Table01", "Table02", "Table5", "Table0100", "Table550", "Tables1" });
        }

        [Test]
        public void IdentifierSortCases()
        {
            AssertIdentifierSorted(
                new[] { "bbb", "BBB", "aaa", "AAA", "AAAA", "aaaa", "a[a", "a}a" },
                new[] { "a[a", "AAA", "aaa", "AAAA", "aaaa", "a}a", "BBB", "bbb" });
        }

        private void AssertGuidKeyPathSorted(string[] input, string[] output)
        {
            Assert.AreEqual(input.Length, output.Length);
            var rowDatas = new List<CSVRowData>();
            for (int i = 0; i < input.Length; i++)
                rowDatas.Add(new CSVRowData(null, null, input[i]));

            var outputRows = CSVUtil.GuidKeyPathSort(rowDatas).ToList();
            Assert.AreEqual(output.Length, outputRows.Count);

            for (int i = 0; i < output.Length; i++)
                Assert.AreEqual(output[i], outputRows[i].GUID);
        }

        [Test]
        public void KeyPathSortOnKeyPaths()
        {
            AssertGuidKeyPathSorted(
                new[]
                {
                    "EventInfo[Adventure].EventReward3.Transaction",
                    "EventInfo[Adventure].EventReward1.Transaction",
                    "EventInfo[Adventure].EventReward2.Transaction"
                },
                new[]
                {
                    "EventInfo[Adventure].EventReward1.Transaction",
                    "EventInfo[Adventure].EventReward2.Transaction",
                    "EventInfo[Adventure].EventReward3.Transaction"
                });

            AssertGuidKeyPathSorted(
                new[]
                {
                    "EventInfo[Adventure].EventReward.Transaction3",
                    "EventInfo[Adventure].EventReward.Transaction2",
                    "EventInfo[Adventure].EventReward.Transaction1"
                },
                new[]
                {
                    "EventInfo[Adventure].EventReward.Transaction1",
                    "EventInfo[Adventure].EventReward.Transaction2",
                    "EventInfo[Adventure].EventReward.Transaction3"
                });

            AssertGuidKeyPathSorted(
                new[]
                {
                    "EventInfo[Adventure].EventReward.Abc",
                    "EventInfo[Adventure].AEventReward.Transaction",
                    "EventInfo[Adventure].EventReward.Transaction"
                },
                new[]
                {
                    "EventInfo[Adventure].AEventReward.Transaction",
                    "EventInfo[Adventure].EventReward.Abc",
                    "EventInfo[Adventure].EventReward.Transaction"
                });

            AssertGuidKeyPathSorted(
                new[]
                {
                    "EventInfo[Adventure].EventReward.Transactionss",
                    "EventInfo[Adventure].EventReward.Transaction",
                    "EventInfo[Adventure].EventReward.Transactions"
                },
                new[]
                {
                    "EventInfo[Adventure].EventReward.Transaction",
                    "EventInfo[Adventure].EventReward.Transactions",
                    "EventInfo[Adventure].EventReward.Transactionss"
                });
        }

        [Test]
        public void KeyPathSortOnType()
        {
            AssertGuidKeyPathSorted(
                new[]
                {
                    "SpecialEventInfo[Adventure].EventReward.Transaction",
                    "EventInfo[Adventure].EventReward.Transaction",
                    "MiniGameInfo[Adventure].EventReward.Transaction"
                },
                new[]
                {
                    "EventInfo[Adventure].EventReward.Transaction",
                    "MiniGameInfo[Adventure].EventReward.Transaction",
                    "SpecialEventInfo[Adventure].EventReward.Transaction"
                });

            AssertGuidKeyPathSorted(
                new[]
                {
                    "EventInfoInfoInfo[Adventure].EventReward.Transaction",
                    "EventInfo[Adventure].EventReward.Transaction",
                    "EventInfoInfo[Adventure].EventReward.Transaction",
                },
                new[]
                {
                    "EventInfo[Adventure].EventReward.Transaction",
                    "EventInfoInfo[Adventure].EventReward.Transaction",
                    "EventInfoInfoInfo[Adventure].EventReward.Transaction"
                });
        }

        [Test]
        public void KeyPathSortOnIdentifier()
        {
            AssertGuidKeyPathSorted(
                new[]
                {
                    "EventInfo[Zoo].EventReward.Transaction",
                    "EventInfo[00102Adventure].EventReward.Transaction",
                    "EventInfo[12Adventure].EventReward.Transaction"
                },
                new[]
                {
                    "EventInfo[12Adventure].EventReward.Transaction",
                    "EventInfo[00102Adventure].EventReward.Transaction",
                    "EventInfo[Zoo].EventReward.Transaction",
                });

            AssertGuidKeyPathSorted(
                new[]
                {
                    "EventInfo[Adventure0002].EventReward.Transaction",
                    "EventInfo[Adventure10].EventReward.Transaction",
                    "EventInfo[1000Adventure].EventReward.Transaction"
                },
                new[]
                {
                    "EventInfo[1000Adventure].EventReward.Transaction",
                    "EventInfo[Adventure0002].EventReward.Transaction",
                    "EventInfo[Adventure10].EventReward.Transaction"
                });

            AssertGuidKeyPathSorted(
                new[]
                {
                    "EventInfo[Adventures10].EventReward.Transaction",
                    "EventInfo[Adventures].EventReward.Transaction",
                    "EventInfo[Adventure].EventReward.Transaction"
                },
                new[]
                {
                    "EventInfo[Adventure].EventReward.Transaction",
                    "EventInfo[Adventures].EventReward.Transaction",
                    "EventInfo[Adventures10].EventReward.Transaction"
                });
        }

        [Test]
        public void KeyPathSortNestedArray()
        {
            // check different length indexes
            AssertGuidKeyPathSorted(
                new[]
                {
                    "EventInfo[Adventure].EventReward[1000].Transaction",
                    "EventInfo[Adventure].EventReward[523].Transaction",
                    "EventInfo[Adventure].EventReward[9].Transaction",
                },
                new[]
                {
                    "EventInfo[Adventure].EventReward[9].Transaction",
                    "EventInfo[Adventure].EventReward[523].Transaction",
                    "EventInfo[Adventure].EventReward[1000].Transaction"
                });

            AssertGuidKeyPathSorted(
                new[]
                {
                    "EventInfo[Adventure].EventReward[1].Transaction",
                    "EventInfo[Adventure].EventReward[10].Transaction",
                    "EventInfo[Adventure].EventReward[9].Transaction",
                    "EventInfo[Adventure].EventReward[8].Transaction",
                    "EventInfo[Adventure].EventReward[7].Transaction",
                    "EventInfo[Adventure].EventReward[3].Transaction",
                    "EventInfo[Adventure].EventReward[5].Transaction",
                    "EventInfo[Adventure].EventReward[4].Transaction",
                    "EventInfo[Adventure].EventReward[6].Transaction",
                    "EventInfo[Adventure].EventReward[2].Transaction",
                    "EventInfo[Adventure].EventReward[0].Transaction"
                },
                new[]
                {
                    "EventInfo[Adventure].EventReward[0].Transaction",
                    "EventInfo[Adventure].EventReward[1].Transaction",
                    "EventInfo[Adventure].EventReward[2].Transaction",
                    "EventInfo[Adventure].EventReward[3].Transaction",
                    "EventInfo[Adventure].EventReward[4].Transaction",
                    "EventInfo[Adventure].EventReward[5].Transaction",
                    "EventInfo[Adventure].EventReward[6].Transaction",
                    "EventInfo[Adventure].EventReward[7].Transaction",
                    "EventInfo[Adventure].EventReward[8].Transaction",
                    "EventInfo[Adventure].EventReward[9].Transaction",
                    "EventInfo[Adventure].EventReward[10].Transaction"
                });
            AssertGuidKeyPathSorted(
                new[]
                {
                    "EventInfo[Adventure].EventReward.Transaction[5]",
                    "EventInfo[Adventure].EventReward.Transaction[4]",
                    "EventInfo[Adventure].EventReward.Transaction[3]",
                    "EventInfo[Adventure].EventReward.Transaction[2]",
                    "EventInfo[Adventure].EventReward.Transaction[1]",
                    "EventInfo[Adventure].EventReward.Transaction[0]",
                    "EventInfo[Adventure].EventReward.Transaction[10]",
                    "EventInfo[Adventure].EventReward.Transaction[9]",
                    "EventInfo[Adventure].EventReward.Transaction[8]",
                    "EventInfo[Adventure].EventReward.Transaction[7]",
                    "EventInfo[Adventure].EventReward.Transaction[6]"
                },
                new[]
                {
                    "EventInfo[Adventure].EventReward.Transaction[0]",
                    "EventInfo[Adventure].EventReward.Transaction[1]",
                    "EventInfo[Adventure].EventReward.Transaction[2]",
                    "EventInfo[Adventure].EventReward.Transaction[3]",
                    "EventInfo[Adventure].EventReward.Transaction[4]",
                    "EventInfo[Adventure].EventReward.Transaction[5]",
                    "EventInfo[Adventure].EventReward.Transaction[6]",
                    "EventInfo[Adventure].EventReward.Transaction[7]",
                    "EventInfo[Adventure].EventReward.Transaction[8]",
                    "EventInfo[Adventure].EventReward.Transaction[9]",
                    "EventInfo[Adventure].EventReward.Transaction[10]"
                });

            AssertGuidKeyPathSorted(
                new[]
                {
                    "EventInfo[Adventure].EventReward[1].Transaction[0]",
                    "EventInfo[Adventure].EventReward[1].Transaction[1]",
                    "EventInfo[Adventure].EventReward[1].Transaction[2]",
                    "EventInfo[Adventure].EventReward[1].Transaction[8]",
                    "EventInfo[Adventure].EventReward[1].Transaction[9]",
                    "EventInfo[Adventure].EventReward[1].Transaction[10]",
                    "EventInfo[Adventure].EventReward[2].Transaction[0]",
                    "EventInfo[Adventure].EventReward[6].Transaction[0]",
                    "EventInfo[Adventure].EventReward[7].Transaction[0]",
                    "EventInfo[Adventure].EventReward[8].Transaction[0]",
                    "EventInfo[Adventure].EventReward[1].Transaction[3]",
                    "EventInfo[Adventure].EventReward[1].Transaction[4]",
                    "EventInfo[Adventure].EventReward[1].Transaction[5]",
                    "EventInfo[Adventure].EventReward[1].Transaction[6]",
                    "EventInfo[Adventure].EventReward[1].Transaction[7]",
                    "EventInfo[Adventure].EventReward[9].Transaction[0]",
                    "EventInfo[Adventure].EventReward[10].Transaction[0]",
                    "EventInfo[Adventure].EventReward[0].Transaction[0]",
                    "EventInfo[Adventure].EventReward[0].Transaction[1]",
                    "EventInfo[Adventure].EventReward[3].Transaction[0]",
                    "EventInfo[Adventure].EventReward[4].Transaction[0]",
                    "EventInfo[Adventure].EventReward[5].Transaction[0]",
                    "EventInfo[Adventure].EventReward[0].Transaction[2]",
                },
                new[]
                {
                    // EventReward[0]
                    "EventInfo[Adventure].EventReward[0].Transaction[0]",
                    "EventInfo[Adventure].EventReward[0].Transaction[1]",
                    "EventInfo[Adventure].EventReward[0].Transaction[2]",
                    // EventReward[1]
                    "EventInfo[Adventure].EventReward[1].Transaction[0]",
                    "EventInfo[Adventure].EventReward[1].Transaction[1]",
                    "EventInfo[Adventure].EventReward[1].Transaction[2]",
                    "EventInfo[Adventure].EventReward[1].Transaction[3]",
                    "EventInfo[Adventure].EventReward[1].Transaction[4]",
                    "EventInfo[Adventure].EventReward[1].Transaction[5]",
                    "EventInfo[Adventure].EventReward[1].Transaction[6]",
                    "EventInfo[Adventure].EventReward[1].Transaction[7]",
                    "EventInfo[Adventure].EventReward[1].Transaction[8]",
                    "EventInfo[Adventure].EventReward[1].Transaction[9]",
                    "EventInfo[Adventure].EventReward[1].Transaction[10]",
                    // EventReward[2-10],
                    "EventInfo[Adventure].EventReward[2].Transaction[0]",
                    "EventInfo[Adventure].EventReward[3].Transaction[0]",
                    "EventInfo[Adventure].EventReward[4].Transaction[0]",
                    "EventInfo[Adventure].EventReward[5].Transaction[0]",
                    "EventInfo[Adventure].EventReward[6].Transaction[0]",
                    "EventInfo[Adventure].EventReward[7].Transaction[0]",
                    "EventInfo[Adventure].EventReward[8].Transaction[0]",
                    "EventInfo[Adventure].EventReward[9].Transaction[0]",
                    "EventInfo[Adventure].EventReward[10].Transaction[0]"
                });
        }

        [Test]
        public void KeyPathSortMulti()
        {
            AssertGuidKeyPathSorted(
                new[]
                {
                    "SpecialEventInfo[Adventure].EventReward.Transaction",
                    "CurrencyInfo[Coin].Reward.Transaction",
                    "SpecialEventInfo[Wander].EventReward.Transaction",
                    "EventInfo[Adventure].EventReward.Transaction",
                    "CurrencyInfo[Gem].Reward.Transaction",
                    "EventInfo[Wander].EventReward.Transaction",
                },
                new[] {
                    /*
                     * 1. sort by just KeyPath first - comparing:
                     *  Reward.Transaction
                     *  EventReward.Transaction
                     * 2. sort by type next - comparing:
                     *  EventInfo
                     *  SpecialEventInfo
                     * 3. sort by info identifier
                     *  Adventure
                     *  Wander
                     */
                    "EventInfo[Adventure].EventReward.Transaction",
                    "EventInfo[Wander].EventReward.Transaction",
                    "SpecialEventInfo[Adventure].EventReward.Transaction",
                    "SpecialEventInfo[Wander].EventReward.Transaction",
                    "CurrencyInfo[Coin].Reward.Transaction",
                    "CurrencyInfo[Gem].Reward.Transaction"
                });

            AssertGuidKeyPathSorted(
                new[]
                {
                    "BuildingInfo[GoldMine].GeneratedCurrencies[0].Transaction",
                    "SpecialEventInfo[Adventure].EventReward.Transaction",
                    "BuildingInfo[GoldMine].GeneratedCurrencies[1].Transaction",
                    "CurrencyInfo[Coin].Reward.Transaction",
                    "BuildingInfo[LumberYard].GeneratedCurrencies[0].Transaction",
                    "SpecialEventInfo[Wander].EventReward.Transaction",
                    "BuildingInfo[LumberYard].GeneratedCurrencies[1].Transaction",
                    "EventInfo[Adventure].EventReward.Transaction",
                    "BuildingInfo[LumberYard].GeneratedCurrencies[2].Transaction",
                    "CurrencyInfo[Gem].Reward.Transaction",
                    "EventInfo[Wander].EventReward.Transaction",
                },
                new[] {
                    /*
                     * 1. sort by just KeyPath first - comparing:
                     *  Reward.Transaction
                     *  EventReward.Transaction\
                     * 2. sort by type next - comparing:
                     *  EventInfo
                     *  SpecialEventInfo
                     * 3. sort by info identifier
                     *  Adventure
                     *  Wander
                     * 4. sort by array indexes
                     */
                    "EventInfo[Adventure].EventReward.Transaction",
                    "EventInfo[Wander].EventReward.Transaction",
                    "SpecialEventInfo[Adventure].EventReward.Transaction",
                    "SpecialEventInfo[Wander].EventReward.Transaction",
                    "BuildingInfo[GoldMine].GeneratedCurrencies[0].Transaction",
                    "BuildingInfo[GoldMine].GeneratedCurrencies[1].Transaction",
                    "BuildingInfo[LumberYard].GeneratedCurrencies[0].Transaction",
                    "BuildingInfo[LumberYard].GeneratedCurrencies[1].Transaction",
                    "BuildingInfo[LumberYard].GeneratedCurrencies[2].Transaction",
                    "CurrencyInfo[Coin].Reward.Transaction",
                    "CurrencyInfo[Gem].Reward.Transaction"
                });
        }

        [Test]
        public void KeyPathSortMalformedKeyPaths()
        {
            // missing everything after type
            AssertGuidKeyPathSorted(
                new[]
                {
                    "SpecialEventInfo",
                    "SpecialEventInfo",
                },
                new[]
                {
                    "SpecialEventInfo",
                    "SpecialEventInfo",
                });

            // missing everything after type
            AssertGuidKeyPathSorted(
                new[]
                {
                    "SpecialEventInfoInfo",
                    "SpecialEventInfo",
                    "SpecialEventInfoInfo"
                },
                new[]
                {
                    "SpecialEventInfo",
                    "SpecialEventInfoInfo",
                    "SpecialEventInfoInfo",
                });

            // missing bad identifier brackets
            AssertGuidKeyPathSorted(
                new[]
                {
                    "SpecialEventInfo[",
                    "SpecialEventInfo[Identi",
                    "SpecialEventInfo[Identifier]",
                    "SpecialEventInfo[Identifier]abc",
                    "SpecialEventInfo[Identifier]abcd",
                    "SpecialEventInfo[Identifiers",
                    "SpecialEventInfo[Identi",
                    "SpecialEventInfo[Identifier]",
                    "SpecialEventInfo[",
                },
                new[]
                {
                    "SpecialEventInfo[",
                    "SpecialEventInfo[",
                    "SpecialEventInfo[Identi",
                    "SpecialEventInfo[Identi",
                    "SpecialEventInfo[Identifier]",
                    "SpecialEventInfo[Identifier]",
                    "SpecialEventInfo[Identifiers",
                    "SpecialEventInfo[Identifier]abc",
                    "SpecialEventInfo[Identifier]abcd",
                });

            // missing bad identifier brackets
            AssertGuidKeyPathSorted(
                new[]
                {
                    "SpecialEventInfo[Identifier].SomeArray[0].Blah",
                    "SpecialEventInfo[Identifier].SomeArray[1].Blah",
                    "SpecialEventInfo[Identifier].SomeArray[2]",
                    "SpecialEventInfo[Identifier].SomeArray[88",
                    "SpecialEventInfo[Identifier].SomeArray[99",
                    "SpecialEventInfo[Identifier].SomeArray[130",
                },
                new[]
                {
                    "SpecialEventInfo[Identifier].SomeArray[88",
                    "SpecialEventInfo[Identifier].SomeArray[99",
                    "SpecialEventInfo[Identifier].SomeArray[130",
                    "SpecialEventInfo[Identifier].SomeArray[2]",
                    "SpecialEventInfo[Identifier].SomeArray[0].Blah",
                    "SpecialEventInfo[Identifier].SomeArray[1].Blah",
                });
        }
    }
}
