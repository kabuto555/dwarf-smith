using UnityEngine;

namespace MonkeyBear.Game
{
    public class ActionIndicator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer ArrowSprite;

        public Color IndicatorColor
        {
            set => ArrowSprite.color = value;
            get => ArrowSprite.color;
        }

        public void SetAimDirection(int cellDirection)
        {
            var rotation = Quaternion.Euler(0, 0, cellDirection * 90);
            transform.rotation = rotation;
        }
    }
}
