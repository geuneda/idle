using System;

namespace IdleRPG.Mine
{
    /// <summary>
    /// 광산 보드 생성 로직을 담당한다.
    /// </summary>
    internal class MineBoardGenerator
    {
        private readonly Random _random;

        /// <summary>
        /// <see cref="MineBoardGenerator"/>를 생성한다.
        /// </summary>
        /// <param name="random">공유 난수 생성기</param>
        internal MineBoardGenerator(Random random)
        {
            _random = random;
        }

        /// <summary>
        /// 보드를 리셋하고 블록을 무작위 생성한다.
        /// </summary>
        /// <param name="model">광산 모델</param>
        /// <param name="weightConfig">층별 블록 가중치 설정</param>
        internal void GenerateBoard(MineModel model, MineBlockWeightConfig weightConfig)
        {
            model.ResetBoard();
            if (weightConfig == null) return;

            int chestPos = _random.Next(MineModel.BoardSize);
            model.SetChestIndex(chestPos);
            model.SetBoardBlock(chestPos, new MineBlock(
                MineBlockState.Hidden, MineBlockRewardType.TreasureChest, 1));

            for (int i = 0; i < MineModel.BoardSize; i++)
            {
                if (i == chestPos) continue;
                model.SetBoardBlock(i, GenerateRandomBlock(weightConfig));
            }
        }

        /// <summary>
        /// 가중치 설정에 따라 무작위 블록을 생성한다.
        /// </summary>
        /// <param name="weightConfig">블록 가중치 설정</param>
        /// <returns>생성된 블록</returns>
        internal MineBlock GenerateRandomBlock(MineBlockWeightConfig weightConfig)
        {
            int totalWeight = weightConfig.TotalWeight;
            if (totalWeight <= 0)
                return new MineBlock(MineBlockState.Hidden, MineBlockRewardType.Empty, 0);

            int roll = _random.Next(totalWeight);
            int cumulative = 0;

            cumulative += weightConfig.WeightEmpty;
            if (roll < cumulative)
                return new MineBlock(MineBlockState.Hidden, MineBlockRewardType.Empty, 0);

            cumulative += weightConfig.WeightOre;
            if (roll < cumulative)
            {
                int amount = _random.Next(weightConfig.OreMinAmount, weightConfig.OreMaxAmount + 1);
                return new MineBlock(MineBlockState.Hidden, MineBlockRewardType.Ore, amount);
            }

            cumulative += weightConfig.WeightPickaxe;
            if (roll < cumulative)
                return new MineBlock(MineBlockState.Hidden, MineBlockRewardType.Pickaxe, weightConfig.PickaxeAmount);

            cumulative += weightConfig.WeightBomb;
            if (roll < cumulative)
                return new MineBlock(MineBlockState.Hidden, MineBlockRewardType.Bomb, weightConfig.BombAmount);

            cumulative += weightConfig.WeightDynamite;
            if (roll < cumulative)
                return new MineBlock(MineBlockState.Hidden, MineBlockRewardType.Dynamite, weightConfig.DynamiteAmount);

            return new MineBlock(MineBlockState.Hidden, MineBlockRewardType.ExpFragment, weightConfig.ExpFragmentAmount);
        }
    }
}
