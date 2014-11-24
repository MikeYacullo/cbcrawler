public class Address
{
	private int _X;
	public int x {
		get {
			return this._X;
		}
		set {
			_X = value;
		}
	}
	
	private int _Y;
	public int y {
		get {
			return this._Y;
		}
		set {
			_Y = value;
		}
	}
	
	public Address (int _X, int _Y)
	{
		this._X = _X;
		this._Y = _Y;
	}
	
	public bool Equals (Address a)
	{
		bool condition_0 = false;
		if (_X == a.x)
			condition_0 = true;
		bool condition_1 = false;
		if (_Y == a.y)
			condition_1 = true;
		
		if (condition_0 && condition_1)
			return true;
		else
			return false;
	}
	
	public override string ToString ()
	{
		return x + "," + y;
	}
}