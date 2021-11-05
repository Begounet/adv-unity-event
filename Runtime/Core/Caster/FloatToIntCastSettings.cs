using System.Collections;
using UnityEngine;

namespace AUE
{
    [System.Serializable]
    public class FloatToIntCastSettings : ICastSettings
    {
        public enum EMode
        {
            Round,
            Ceil,
            Floor
        }

        [SerializeField]
        private EMode _mode = EMode.Round;
        public EMode Mode { get => _mode; set => _mode = value; }

        public int Cast(float v)
        {
            switch (_mode)
            {
                case EMode.Round:
                    return Mathf.RoundToInt(v);
                case EMode.Ceil:
                    return Mathf.CeilToInt(v);
                case EMode.Floor:
                    return Mathf.FloorToInt(v);
            }
            return (int)v;
        }
    }
}