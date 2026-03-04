using System.Collections.Generic;
using Geuneda.DataExtensions;
using Geuneda.Services;
using IdleRPG.Core;
using IdleRPG.Economy;
using IdleRPG.OfflineReward;
using IdleRPG.Reward;
using IdleRPG.Stage;
using NUnit.Framework;

namespace IdleRPG.Tests.EditMode
{
    /// <summary>
    /// <see cref="OfflineRewardService"/> 로직 테스트.
    /// </summary>
    [TestFixture]
    public class OfflineRewardServiceTest
    {
        private OfflineRewardService _service;
        private OfflineRewardConfig _offlineConfig;
        private RewardConfig _rewardConfig;
        private StageConfig _stageConfig;
        private StubTimeService _timeService;
        private StubStageService _stageService;
        private MockCurrencyService _currencyService;
        private MockMessageBroker _messageBroker;

        [SetUp]
        public void Setup()
        {
            _offlineConfig = new OfflineRewardConfig
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
                        MaxGlobalStageIndex = 99,
                        DropEntries = new List<OfflineDropEntry>
                        {
                            new OfflineDropEntry
                            {
                                DropType = OfflineDropType.Pet,
                                Grade = OfflineDropGrade.Common,
                                DropChancePerMinute = 0.05f
                            }
                        }
                    }
                }
            };

            _rewardConfig = new RewardConfig
            {
                BaseGoldPerKill = 10,
                GoldScalePerStage = 0.12f
            };

            _stageConfig = new StageConfig
            {
                StagesPerChapter = 10
            };

            _timeService = new StubTimeService();
            _stageService = new StubStageService(1, 1);
            _currencyService = new MockCurrencyService();
            _messageBroker = new MockMessageBroker();

            _service = new OfflineRewardService(
                _offlineConfig, _rewardConfig, _stageConfig,
                _stageService, _currencyService, _timeService, _messageBroker);
        }

        [Test]
        public void HasOfflineReward_타임스탬프_0이면_false()
        {
            Assert.IsFalse(_service.HasOfflineReward(0));
        }

        [Test]
        public void HasOfflineReward_최소시간_미만이면_false()
        {
            _timeService.SetUnixTimeNow(60000 + 30000);
            Assert.IsFalse(_service.HasOfflineReward(60000));
        }

        [Test]
        public void HasOfflineReward_최소시간_이상이면_true()
        {
            _timeService.SetUnixTimeNow(60000 + 120000);
            Assert.IsTrue(_service.HasOfflineReward(60000));
        }

        [Test]
        public void CalculateReward_최대시간_초과시_클램프()
        {
            long lastSave = 0;
            long twentyHoursLater = (long)(20 * 60 * 60 * 1000);
            _timeService.SetUnixTimeNow(twentyHoursLater);

            var result = _service.CalculateReward(lastSave);

            double maxMinutes = _offlineConfig.DefaultMaxOfflineHours * 60.0;
            Assert.AreEqual(maxMinutes, result.ClampedMinutes, 0.01);
            Assert.IsTrue(result.ElapsedMinutes > maxMinutes);
        }

        [Test]
        public void CalculateReward_현재_스테이지_기반_계산()
        {
            _timeService.SetUnixTimeNow(3600000);
            _stageService.SetStage(2, 5);

            var result = _service.CalculateReward(0);

            Assert.IsTrue(result.Gold > BigNumber.Zero);
        }

        [Test]
        public void ClaimReward_골드_CurrencyService_통해_지급()
        {
            var result = new OfflineRewardResult(
                60, 60,
                new BigNumber(500, 0),
                new List<OfflineDropResult>());

            _service.ClaimReward(result);

            Assert.AreEqual(CurrencyType.Gold, _currencyService.LastAddType);
            Assert.AreEqual(new BigNumber(500, 0), _currencyService.LastAddAmount);
        }

        [Test]
        public void ClaimReward_메시지_발행()
        {
            var result = new OfflineRewardResult(
                60, 60,
                new BigNumber(100, 0),
                new List<OfflineDropResult>());

            _service.ClaimReward(result);

            Assert.IsTrue(_messageBroker.PublishedMessageTypes
                .Contains(typeof(OfflineRewardClaimedMessage)));
        }

        [Test]
        public void ClaimReward_골드_0이면_CurrencyService_호출안함()
        {
            var result = new OfflineRewardResult(
                0, 0,
                BigNumber.Zero,
                new List<OfflineDropResult>());

            _service.ClaimReward(result);

            Assert.IsFalse(_currencyService.AddCalled);
        }

        #region Test Doubles

        private class StubTimeService : ITimeService
        {
            private long _unixTimeNow;

            public long UnixTimeNow => _unixTimeNow;
            public System.DateTime DateTimeUtcNow => System.DateTime.UtcNow;
            public float UnityTimeNow => 0f;
            public float UnityScaleTimeNow => 0f;

            public void SetUnixTimeNow(long value) => _unixTimeNow = value;

            public long UnixTimeFromDateTimeUtc(System.DateTime dt) => 0;
            public long UnixTimeFromUnityTime(float time) => 0;
            public System.DateTime DateTimeUtcFromUnixTime(long unixMs) => System.DateTime.UtcNow;
            public System.DateTime DateTimeUtcFromUnityTime(float time) => System.DateTime.UtcNow;
            public float UnityTimeFromDateTimeUtc(System.DateTime time) => 0f;
            public float UnityTimeFromUnixTime(long unixMs) => 0f;
        }

        private class StubStageService : IStageService
        {
            private readonly StageModel _model;

            public StageModel Model => _model;
            public int WavesPerStage => 5;

            public StubStageService(int chapter, int stage)
            {
                _model = new StageModel();
                _model.CurrentChapter.Value = chapter;
                _model.CurrentStage.Value = stage;
            }

            public void SetStage(int chapter, int stage)
            {
                _model.CurrentChapter.Value = chapter;
                _model.CurrentStage.Value = stage;
            }

            public void StartWave() { }
            public void CompleteWave() { }
            public void FailWave() { }
            public void ToggleBossAutoChallenge(bool enabled) { }
            public bool IsBossWave() => false;
            public bool IsBossWave(int waveIndex) => false;
            public string GetStageDisplayName() => "1-1";
        }

        private class MockCurrencyService : ICurrencyService
        {
            public bool AddCalled { get; private set; }
            public CurrencyType LastAddType { get; private set; }
            public BigNumber LastAddAmount { get; private set; }

            public IObservableDictionaryReader<CurrencyType, BigNumber> Currencies => null;

            public void Add(CurrencyType type, BigNumber amount)
            {
                AddCalled = true;
                LastAddType = type;
                LastAddAmount = amount;
            }

            public bool TrySpend(CurrencyType type, BigNumber amount) => false;
            public bool HasEnough(CurrencyType type, BigNumber amount) => false;
            public BigNumber GetAmount(CurrencyType type) => BigNumber.Zero;
        }

        private class MockMessageBroker : IMessageBrokerService
        {
            public List<System.Type> PublishedMessageTypes { get; } = new List<System.Type>();

            public void Publish<T>(T message) where T : IMessage
            {
                PublishedMessageTypes.Add(typeof(T));
            }

            public void PublishSafe<T>(T message) where T : IMessage
            {
                PublishedMessageTypes.Add(typeof(T));
            }

            public void Subscribe<T>(System.Action<T> action) where T : IMessage { }
            public void Unsubscribe<T>(object subscriber = null) where T : IMessage { }
            public void UnsubscribeAll(object subscriber = null) { }
        }

        #endregion
    }
}
