using System.Collections.Generic;
using Geuneda.DataExtensions;
using IdleRPG.Battle;
using IdleRPG.Core;
using IdleRPG.Growth;
using IdleRPG.Hero;
using IdleRPG.Reward;
using IdleRPG.Stage;
using NUnit.Framework;

namespace IdleRPG.Tests.EditMode
{
    /// <summary>
    /// <see cref="ConfigsProvider"/>를 통한 Config 등록/조회 통합 테스트.
    /// </summary>
    [TestFixture]
    public class ConfigsProviderIntegrationTest
    {
        private ConfigsProvider _provider;

        [SetUp]
        public void Setup()
        {
            _provider = new ConfigsProvider();
        }

        [Test]
        public void HeroConfig_싱글톤_등록_후_조회()
        {
            var config = new HeroConfig { BaseHp = 200, BaseAttack = 20 };
            _provider.AddSingletonConfig(config);

            var retrieved = _provider.GetConfig<HeroConfig>();

            Assert.AreEqual(200, retrieved.BaseHp);
            Assert.AreEqual(20, retrieved.BaseAttack);
        }

        [Test]
        public void StageConfig_싱글톤_등록_후_조회()
        {
            var config = new StageConfig { StagesPerChapter = 5 };
            _provider.AddSingletonConfig(config);

            var retrieved = _provider.GetConfig<StageConfig>();

            Assert.AreEqual(5, retrieved.StagesPerChapter);
        }

        [Test]
        public void EnemyConfig_컬렉션_ID별_조회()
        {
            var configs = new List<EnemyConfig>
            {
                new EnemyConfig { Id = 1, BaseHp = 30, IsBoss = false },
                new EnemyConfig { Id = 100, BaseHp = 200, IsBoss = true }
            };

            _provider.AddConfigs(c => c.Id, (IList<EnemyConfig>)configs);

            var normal = _provider.GetConfig<EnemyConfig>(1);
            Assert.AreEqual(30, normal.BaseHp);
            Assert.IsFalse(normal.IsBoss);

            var boss = _provider.GetConfig<EnemyConfig>(100);
            Assert.AreEqual(200, boss.BaseHp);
            Assert.IsTrue(boss.IsBoss);
        }

        [Test]
        public void GrowthConfig_싱글톤_등록_후_BuildLookup_동작()
        {
            var config = new GrowthConfig();
            config.BuildLookup();
            _provider.AddSingletonConfig(config);

            var retrieved = _provider.GetConfig<GrowthConfig>();
            var attackEntry = retrieved.GetEntry(HeroStatType.Attack);

            Assert.IsNotNull(attackEntry);
            Assert.AreEqual(HeroStatType.Attack, attackEntry.StatType);
            Assert.AreEqual(10, attackEntry.BaseValue);
        }

        [Test]
        public void RewardConfig_싱글톤_등록_후_조회()
        {
            var config = new RewardConfig { BaseGoldPerKill = 15 };
            _provider.AddSingletonConfig(config);

            var retrieved = _provider.GetConfig<RewardConfig>();

            Assert.AreEqual(15, retrieved.BaseGoldPerKill);
        }

        [Test]
        public void HeroConfig_double_BaseHp_BigNumber_암시적_변환()
        {
            var config = new HeroConfig { BaseHp = 100 };

            BigNumber hp = config.BaseHp;

            Assert.AreEqual(new BigNumber(100, 0), hp);
        }

        [Test]
        public void EnemyConfig_double_BaseHp_곱셈_BigNumber_변환()
        {
            var config = new EnemyConfig { BaseHp = 30 };
            BigNumber multiplier = 2.0;

            BigNumber result = config.BaseHp * multiplier;

            Assert.AreEqual(new BigNumber(60, 0), result);
        }

        [Test]
        public void RewardConfig_CalculateGoldReward_double필드_정상동작()
        {
            var config = new RewardConfig
            {
                BaseGoldPerKill = 10,
                BossGoldMultiplier = 5f,
                GoldScalePerStage = 0.12f
            };

            BigNumber gold = config.CalculateGoldReward(1, 1, 10, false);

            Assert.IsTrue(gold > BigNumber.Zero);
        }
    }
}
