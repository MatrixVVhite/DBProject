using System.Data;

namespace Server.DataStructures
{
	public interface IFromDataRow
	{
		void FromDataRow(DataRow row);
	}
}
