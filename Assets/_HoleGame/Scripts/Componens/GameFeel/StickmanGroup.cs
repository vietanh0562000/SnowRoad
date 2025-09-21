namespace HoleBox
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class StickmanGroup : Singleton<StickmanGroup>
    {
        public List<Stickman> characters  = new List<Stickman>();
        public float          minInterval = 1.5f;
        public float          maxInterval = 4f;

        public int minCount = 1;
        public int maxCount = 2;

        public void AddStickman(Stickman stickman) { characters.Add(stickman); }

        public void RemoveStickman(Stickman stickman) { characters.Remove(stickman); }
        
        private void ValidateGroup()
        {
            foreach (var character in characters.ToList())
            {
                if (character == null || !character.gameObject.activeSelf)
                {
                    characters.Remove(character);
                }
            }
        }

        private float timer;

        void Update()
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                ValidateGroup();

                if (characters.Count > 0)
                {
                    // Random number of stickmen to wave: between 2 and 4, but not more than the available count
                    int waveCount = Mathf.Min(Random.Range(minCount, maxCount + 1), characters.Count);

                    // Create a list of indices to select unique stickmen
                    List<int> availableIndices = Enumerable.Range(0, characters.Count).ToList();

                    for (int i = 0; i < waveCount; i++)
                    {
                        // Select a random index from availableIndices
                        int randomListIndex = Random.Range(0, availableIndices.Count);
                        int characterIndex  = availableIndices[randomListIndex];

                        // Make the chosen stickman wave
                        characters[characterIndex].PlayWave();

                        // Remove the chosen index to avoid duplicates
                        availableIndices.RemoveAt(randomListIndex);
                    }
                }

                ResetTimer();
            }
        }

        void ResetTimer() { timer = Random.Range(minInterval, maxInterval); }
    }
}