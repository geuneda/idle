using System;

namespace IdleRPG.Mine
{
    /// <summary>
    /// 층 구간별 블록 보상 가중치 설정.
    /// FloorStart 이상의 층에 적용되며, 다음 구간 시작 전까지 유효하다.
    /// </summary>
    [Serializable]
    public class MineBlockWeightConfig
    {
        /// <summary>이 설정이 적용되는 시작 층 (1, 10, 20 등)</summary>
        public int FloorStart = 1;

        /// <summary>빈칸 가중치</summary>
        public int WeightEmpty = 40;

        /// <summary>광물 가중치</summary>
        public int WeightOre = 25;

        /// <summary>광물 최소 수량</summary>
        public int OreMinAmount = 1;

        /// <summary>광물 최대 수량</summary>
        public int OreMaxAmount = 3;

        /// <summary>곡괭이 가중치</summary>
        public int WeightPickaxe = 15;

        /// <summary>곡괭이 획득 수량</summary>
        public int PickaxeAmount = 1;

        /// <summary>폭탄 가중치</summary>
        public int WeightBomb = 8;

        /// <summary>폭탄 획득 수량</summary>
        public int BombAmount = 1;

        /// <summary>다이너마이트 가중치</summary>
        public int WeightDynamite = 4;

        /// <summary>다이너마이트 획득 수량</summary>
        public int DynamiteAmount = 1;

        /// <summary>경험치 파편 가중치</summary>
        public int WeightExpFragment = 8;

        /// <summary>경험치 파편 획득 수량</summary>
        public int ExpFragmentAmount = 1;

        /// <summary>
        /// 전체 가중치 합계를 반환한다.
        /// </summary>
        public int TotalWeight =>
            WeightEmpty + WeightOre + WeightPickaxe + WeightBomb +
            WeightDynamite + WeightExpFragment;
    }
}
