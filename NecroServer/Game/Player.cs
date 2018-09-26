using System;
using System.Collections.Generic;
using System.Text;
using GameMath;
using NecroServer;

namespace Game
{
    public class Player
    {
        public long UserId { get; }
        public bool IsAI { get => UserId < 0; }
        public string Name { get; }

        public List<Unit> Units { get; private set; } = new List<Unit>();
        public Vector2 AvgPosition { get; private set; } = new Vector2(0, 0);
        public RuneType UnitsRune { get; private set; } = RuneType.None;

        private Vector2 Input = new Vector2(0, 0);
        private DateTime lastRune = DateTime.Now;

        private readonly Config Config;

        public Player(long userId, Config config)
        {
            Config = config;
            UserId = userId;
        }

        public void SetInput(Vector2 input)
        {
            Input = input;
        }

        public void Update()
        {

        }

        public void SetRune(RuneType rune)
        {
            lastRune = DateTime.Now;
            UnitsRune = rune;
        }
    }
}
