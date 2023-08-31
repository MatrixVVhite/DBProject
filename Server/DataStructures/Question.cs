using MySql.Data.MySqlClient;
using System.Data;

namespace Server.DataStructures
{
	[Serializable]
	public struct Question : IFromDataRow
	{
		public int ID { get; set; }
		public string QuestionText { get; set; }
		public int CorrectAnswer { get; set; }
		public string Answer1 { get; set; }
		public string Answer2 { get; set; }
		public string Answer3 { get; set; }
		public string Answer4 { get; set; }

		public Question() : this(0, string.Empty, 0, string.Empty, string.Empty, string.Empty, string.Empty) { }

		public Question(int id, string question, int correctAnswer, string answer1, string answer2, string answer3, string answer4)
		{
			ID = id;
			QuestionText = question;
			CorrectAnswer = correctAnswer;
			Answer1 = answer1;
			Answer2 = answer2;
			Answer3 = answer3;
			Answer4 = answer4;
		}

		public Question(DataRow row) : this(
			int.Parse(row["QuestionID"].ToString()!),
			(string)row["QuestionText"],
			int.Parse(row["CorrectAnswer"].ToString()!),
			(string)row["Answer1"],
			(string)row["Answer2"],
			(string)row["Answer3"],
			(string)row["Answer4"])
		{ }

		public void FromDataRow(DataRow row) => this = new(row);
	}
}
 