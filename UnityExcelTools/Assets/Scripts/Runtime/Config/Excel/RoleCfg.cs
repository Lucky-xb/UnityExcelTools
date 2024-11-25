public class RoleCfg: PickData
{
		/// <summary>
		/// 编号
		/// <summary>
		public int Id;
		/// <summary>
		/// 名称
		/// <summary>
		public string Name;
		/// <summary>
		/// 介绍
		/// <summary>
		public string Decs;


	/// <summary>
	/// 解析数据
	/// <summary>
	public override void ReadData(byte[] datas, ref int index, ref int uid)
	{
			Id = PickData.ReadInt(datas, ref index);
			Name = PickData.ReadString(datas, ref index);
			Decs = PickData.ReadString(datas, ref index);
			uid = Id;
	}
}
