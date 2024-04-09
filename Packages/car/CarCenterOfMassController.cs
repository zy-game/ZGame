using System;
using CarLogic;
using UnityEngine;

[Serializable]
public class CarCenterOfMassController : ControllerBase
{
	private Rigidbody _carRigidbody;

	private Vector3 _centerOfMass = new Vector3(0f, 0f, 0f);

	public Vector3 _turnOffset = new Vector3(0f, 0f, 0f);

	public Vector3 _driftOffset = new Vector3(0f, 0f, 0f);

	public void SetRigibody(Rigidbody obj)
	{
		_carRigidbody = obj;
		_centerOfMass = _carRigidbody.centerOfMass;
	}

	public void ApplyCenterOfMassOffset(float steer, CarState carState)
	{
		if (carState != null && null != _carRigidbody && carState.CurDriftState != null)
		{
			if (carState.CurDriftState.Stage == DriftStage.NONE)
			{
				_carRigidbody.centerOfMass = _centerOfMass + _turnOffset * Math.Abs(steer);
			}
			else
			{
				_carRigidbody.centerOfMass = _centerOfMass + _driftOffset * Math.Abs(steer);
			}
		}
	}
}
