static class Utility
{
	public static float ClampMin(float val, float min)
	{
		return Math.Max(val, min);
	}

	public static float ClampMax(float val, float max)
	{
		return Math.Min(val, max);
	}

	public static float ClampRange(float val, float min, float max)
	{
		return ClampMin(ClampMax(val, max), min);
	}

	public static int ClampMin(int val, int min)
	{
		return Math.Max(val, min);
	}

	public static int ClampMax(int val, int max)
	{
		return Math.Min(val, max);
	}

	public static int ClampRange(int val, int min, int max)
	{
		return ClampMin(ClampMax(val, max), min);
	}
}