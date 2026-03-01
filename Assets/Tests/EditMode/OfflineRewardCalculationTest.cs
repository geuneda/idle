using System;
using System.Collections.Generic;
using IdleRPG.Core;
using IdleRPG.OfflineReward;
using IdleRPG.Reward;
using NUnit.Framework;

namespace IdleRPG.Tests.EditMode
{
    /// <summary>
    /// <see cref="OfflineRewardConfig"/>의 순수 계산 함수 테스트.
    /// </summary>
    [TestFixture]
    public class OfflineRewardCalculationTest
    {
        private OfflineRewardConfig _config;
        private RewardConfig _rewardConfig;
        private const int StagesPerChapter = 10;

        [SetUp]
        public void Setup()
        {
            _rewardConfig = new RewardConfig
            {
                BaseGoldPerKill = 10,
                BossGoldMultiplier = 5f,
                GoldScalePerStage = 0.12f
            };

            _config = new OfflineRewardConfig
            {
                GoldPerMinuteMultiplier = 10f,
                DefaultMaxOfflineHours = 12f,
                MinOfflineMinutes = 1f,
                AdMultiplier = 2f,
                DropTables = new List<OfflineDropTableEntry>
                {
                    new OfflineDropTableEntry
                    {
                        MinGlobalStageIndex = 0,
                        MaxGlobalStageIndex = 9,
                        DropEntries = new List<OfflineDropEntry>
                        {
                            new OfflineDropEntry
                            {
                                DropType = OfflineDropType.Pet,
                                Grade = ItemGrade.Common,
                                DropChancePerMinute = 0.05f
                            },
                            new OfflineDropEntry
                            {
                                DropType = OfflineDropType.Equipment,
                                Grade = ItemGrade.Common,
                                DropChancePerMinute = 0.03f
                            }
                        }
                    },
                    new OfflineDropTableEntry
                    {
                        MinGlobalStageIndex = 10,
                        MaxGlobalStageIndex = 19,
                        DropEntries = new List<OfflineDropEntry>
                        {
                            new OfflineDropEntry
                            {
                                DropType = OfflineDropType.Pet,
                                Grade = ItemGrade.Uncommon,
                                DropChancePerMinute = 0.04f
                            },
                            new OfflineDropEntry
                            {
                                DropType = OfflineDropType.Skill,
                                Grade = ItemGrade.Rare,
                                DropChancePerMinute = 0.01f
                            }
                        }
                    }
                }
            };
        }

        [Test]
        public void CalculateOfflineGold_경과시간_0이면_골드_0()
        {
            BigNumber gold = _config.CalculateOfflineGold(
                0, _rewardConfig, 1, 1, StagesPerChapter);

            Assert.AreEqual(BigNumber.Zero, gold);
        }

        [Test]
        public void CalculateOfflineGold_1분_경과시_기본골드_곱하기_배율()
        {
            BigNumber gold = _config.CalculateOfflineGold(
                1.0, _rewardConfig, 1, 1, StagesPerChapter);

            BigNumber baseGold = _rewardConfig.CalculateGoldReward(1, 1, StagesPerChapter, false);
            BigNumber expected = BigNumber.Floor(baseGold * _config.GoldPerMinuteMultiplier * 1.0);

            Assert.AreEqual(expected, gold);
        }

        [Test]
        public void CalculateOfflineGold_높은스테이지_더많은_골드()
        {
            BigNumber goldStage1 = _config.CalculateOfflineGold(
                60, _rewardConfig, 1, 1, StagesPerChapter);
            BigNumber goldStage10 = _config.CalculateOfflineGold(
                60, _rewardConfig, 1, 10, StagesPerChapter);

            Assert.IsTrue(goldStage10 > goldStage1);
        }

        [Test]
        public void CalculateOfflineGold_60분_양수_결과()
        {
            BigNumber gold = _config.CalculateOfflineGold(
                60, _rewardConfig, 1, 1, StagesPerChapter);

            Assert.IsTrue(gold > BigNumber.Zero);
        }

        [Test]
        public void FindDropTable_범위내_인덱스_정상_반환()
        {
            var table = _config.FindDropTable(5);

            Assert.IsNotNull(table);
            Assert.AreEqual(0, table.MinGlobalStageIndex);
            Assert.AreEqual(9, table.MaxGlobalStageIndex);
        }

        [Test]
        public void FindDropTable_두번째_범위_정상_반환()
        {
            var table = _config.FindDropTable(15);

            Assert.IsNotNull(table);
            Assert.AreEqual(10, table.MinGlobalStageIndex);
            Assert.AreEqual(19, table.MaxGlobalStageIndex);
        }

        [Test]
        public void FindDropTable_범위외_인덱스_null_반환()
        {
            var table = _config.FindDropTable(100);

            Assert.IsNull(table);
        }

        [Test]
        public void CalculateDrops_경과시간_0이면_빈_목록()
        {
            var random = new Random(42);
            var drops = _config.CalculateDrops(0, 1, 1, StagesPerChapter, random);

            Assert.AreEqual(0, drops.Count);
        }

        [Test]
        public void CalculateDrops_시드_고정시_결정적_결과()
        {
            var drops1 = _config.CalculateDrops(
                60, 1, 1, StagesPerChapter, new Random(42));
            var drops2 = _config.CalculateDrops(
                60, 1, 1, StagesPerChapter, new Random(42));

            Assert.AreEqual(drops1.Count, drops2.Count);
            for (int i = 0; i < drops1.Count; i++)
            {
                Assert.AreEqual(drops1[i].DropType, drops2[i].DropType);
                Assert.AreEqual(drops1[i].Grade, drops2[i].Grade);
                Assert.AreEqual(drops1[i].Count, drops2[i].Count);
            }
        }

        [Test]
        public void CalculateDrops_충분한시간_드롭_발생()
        {
            var random = new Random(42);
            var drops = _config.CalculateDrops(
                480, 1, 1, StagesPerChapter, random);

            Assert.IsTrue(drops.Count > 0);
        }

        [Test]
        public void CalculateDrops_매칭_테이블_없으면_빈_목록()
        {
            var random = new Random(42);
            var drops = _config.CalculateDrops(
                480, 50, 1, StagesPerChapter, random);

            Assert.AreEqual(0, drops.Count);
        }

        [Test]
        public void WithMultiplier_골드_2배_적용()
        {
            var result = new OfflineRewardResult(
                60, 60,
                new BigNumber(1000, 0),
                new List<OfflineDropResult>());

            var doubled = result.WithMultiplier(2f);

            Assert.AreEqual(BigNumber.Floor(new BigNumber(1000, 0) * 2f), doubled.Gold);
        }

        [Test]
        public void WithMultiplier_아이템수량_2배_적용()
        {
            var drops = new List<OfflineDropResult>
            {
                new OfflineDropResult
                {
                    DropType = OfflineDropType.Pet,
                    Grade = ItemGrade.Common,
                    Count = 5
                }
            };

            var result = new OfflineRewardResult(60, 60, new BigNumber(100, 0), drops);
            var doubled = result.WithMultiplier(2f);

            Assert.AreEqual(10, doubled.Drops[0].Count);
        }

        [Test]
        public void WithMultiplier_IsDoubled_true()
        {
            var result = new OfflineRewardResult(
                60, 60,
                new BigNumber(100, 0),
                new List<OfflineDropResult>());

            Assert.IsFalse(result.IsDoubled);

            var doubled = result.WithMultiplier(2f);

            Assert.IsTrue(doubled.IsDoubled);
        }
    }
}
