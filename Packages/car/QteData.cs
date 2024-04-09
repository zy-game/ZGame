using System.IO;

public class QteData
{
	public float Total;

	public float Exact;

	public float Range;

	public float PerfectRange;

	public float ClickTime;

	public QteData()
	{
	}

	public QteData(float total, float exact, float range, float perfectRange, float clickTime)
	{
		Total = total;
		Exact = exact;
		Range = range;
		PerfectRange = perfectRange;
		ClickTime = clickTime;
	}

	public void Read(BinaryReader reader)
	{
		Total = reader.ReadSingle();
		Exact = reader.ReadSingle();
		Range = reader.ReadSingle();
		PerfectRange = reader.ReadSingle();
		ClickTime = reader.ReadSingle();
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write(Total);
		writer.Write(Exact);
		writer.Write(Range);
		writer.Write(PerfectRange);
		writer.Write(ClickTime);
	}
}
