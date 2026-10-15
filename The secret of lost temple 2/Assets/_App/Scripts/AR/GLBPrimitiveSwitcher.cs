using UnityEngine;

namespace SecretOfLostTemple.AR
{
    /// <summary>
    /// Legacy component kept for scene backwards compatibility.
    /// All 3D Simple Machine models (Beam, Boulder, Fulcrum2) use native URP/Lit materials directly.
    /// </summary>
    public class GLBPrimitiveSwitcher : MonoBehaviour
    {
        private void Awake()
        {
            // Do not alter materials at runtime
        }
    }
}

