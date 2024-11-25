public class LoginCfg: PickData
{
		/// <summary>
		/// 登录id
		/// <summary>
		public int Id;
		/// <summary>
		/// 角色名称
		/// <summary>
		public string Name;


	/// <summary>
	/// 解析数据
	/// <summary>
	public override void ReadData(byte[] datas, ref int index, ref int uid)
	{
			Id = PickData.ReadInt(datas, ref index);
			Name = PickData.ReadString(datas, ref index);
			uid = Id;
	}
}
