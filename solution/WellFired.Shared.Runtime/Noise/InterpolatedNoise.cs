using System;
using System.Collections;
using UnityEngine;

namespace WellFired.Shared
{
	public class InterpolatedNoise
	{	
		public Fractal Fractal
		{
			get;
			set;
		}

		public int Seed
		{
			get;
			set;
		}

		public InterpolatedNoise(int seed)
		{
			Seed = seed;
			Fractal = new Fractal(Seed, 1.27f, 2.04f, 8.36f);
		}
		
		public Vector3 GetVector3(float speed, float time)
		{
			var deltaTime = time * 0.01f * speed;
			return new Vector3(Fractal.HybridMultifractal(deltaTime, 15.73f, 0.0f), Fractal.HybridMultifractal(deltaTime, 63.94f, 0.0f), Fractal.HybridMultifractal(deltaTime, 0.2f, 0.0f));
		}
		
		public float GetFloat(float speed, float time)
		{
			var deltaTime = time * 0.01f * speed;
			return Fractal.HybridMultifractal(deltaTime, 15.7f, 0.65f);
		}
	}
}