using System;
using UnityEngine;

public static class RigidbodyTool
{
	public static bool PrintLog;

	public static void SetConstraints(CarView view, RigidbodyConstraints constraints)
	{
		if (PrintLog)
		{
			Debug.Log($">>>> CarId: {view.CarId}, CarType: {view.CarPlayerType}, SetConstraints: {constraints} {PrintConstraints(constraints)}");
		}
		view.GetComponent<Rigidbody>().constraints = constraints;
	}

	public static void AppendConstraints(CarView view, RigidbodyConstraints constraints)
	{
		if (PrintLog)
		{
			Debug.Log($">>>> CarId: {view.CarId}, CarType: {view.CarPlayerType}, AppendConstraints: {constraints} {PrintConstraints(constraints)}");
		}
		view.GetComponent<Rigidbody>().constraints |= constraints;
	}

	public static void RemoveConstraints(CarView view, RigidbodyConstraints constraints)
	{
		if (PrintLog)
		{
			Debug.Log($">>>> CarId: {view.CarId}, CarType: {view.CarPlayerType}, RemoveConstraints: {constraints} {PrintConstraints(constraints)}");
		}
		view.GetComponent<Rigidbody>().constraints &= ~constraints;
	}

	private static string PrintConstraints(RigidbodyConstraints constraints)
	{
		if (constraints == RigidbodyConstraints.None)
		{
			return constraints.ToString();
		}
		string text = "";
		Array values = Enum.GetValues(typeof(RigidbodyConstraints));
		foreach (object item in values)
		{
			if (constraints > RigidbodyConstraints.None && (constraints | (RigidbodyConstraints)item) == constraints)
			{
				text = text + Enum.GetName(typeof(RigidbodyConstraints), item) + "|";
			}
		}
		text = (text.EndsWith("|") ? text.Substring(0, text.Length - 1) : text);
		return $"[ {text} ]";
	}
}
