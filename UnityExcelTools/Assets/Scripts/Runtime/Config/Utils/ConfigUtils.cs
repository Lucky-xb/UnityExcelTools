using System.Collections.Generic;

/// <summary>
/// 工具自动生成，请勿手动修改
/// </summary>
public static class ConfigUtils
{
    private static Dictionary<string, Dictionary<int, PickData>> _cfgDict = new Dictionary<string, Dictionary<int, PickData>>();

		/// <summary>
		/// D登录表_login
		/// <summary>
		public static Dictionary<int, PickData> Logins { get { return GetConfig("Login"); } }
		/// <summary>
		/// J角色表_role
		/// <summary>
		public static Dictionary<int, PickData> Roles { get { return GetConfig("Role"); } }


    public static void InitConfig(string name, byte[] datas)
    {
        switch (name)
        {
				case "Login":
					_cfgDict.Add(name, Convert(ParseData<LoginCfg>(datas)));
					break;
				case "Role":
					_cfgDict.Add(name, Convert(ParseData<RoleCfg>(datas)));
					break;

        }
    }

    private static Dictionary<int, T> ParseData<T>(byte[] data) where T : PickData, new()
    {
        int index = 0;
        int uid = 0;
        int count = PickData.ReadInt(data, ref index);            
        Dictionary<int, T> results = new Dictionary<int, T>();

        for (int i = 0; i < count; i++)
        {
            T item = new T();
            item.ReadData(data, ref index, ref uid);
            results.Add(uid, item);
        }

        return results;
    }

    private static Dictionary<int, PickData> Convert<T>(Dictionary<int, T> datas) where T : PickData, new()
	{
		Dictionary<int, PickData> results = new Dictionary<int, PickData>();
		foreach (var kv in datas)
		{
			results.Add(kv.Key, kv.Value);
		}

		return results;
	}

    public static Dictionary<int, PickData> GetConfig(string name)
    {
        if (_cfgDict.TryGetValue(name, out var results))
        {
             return results;
        }

        return null;
    }

    public static void Clear()
    {
        _cfgDict.Clear();
    }
}
