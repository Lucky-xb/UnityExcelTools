using UnityEngine;

public class Game : MonoBehaviour
{
    [SerializeField]
    private ConfigAsset _configAsset;

    void Start()
    {
        LoadConfig();
    }

    private void LoadConfig()
    {  
        foreach (var item in _configAsset.configs)
        {
            ConfigUtils.InitConfig(item.name, item.bytes);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            var loginCfg = ConfigUtils.Logins[1] as LoginCfg;
            Debug.Log(loginCfg.Name);

            var roleCfg = ConfigUtils.Roles[1] as RoleCfg;
            Debug.Log(roleCfg.Decs);
        }
    }
}
