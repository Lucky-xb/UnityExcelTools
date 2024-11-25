using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/ConfigAsset")]
public class ConfigAsset : ScriptableObject
{
    public TextAsset[] configs;
#if UNITY_EDITOR
    public Object folder;
#endif
}
