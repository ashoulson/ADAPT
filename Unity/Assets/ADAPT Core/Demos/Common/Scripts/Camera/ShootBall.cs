#region License
/*
* Agent Development and Prototyping Testbed
* https://github.com/ashoulson/ADAPT
* 
* Copyright (C) 2011-2015 Alexander Shoulson - ashoulson@gmail.com
*
* This file is part of ADAPT.
* 
* ADAPT is free software: you can redistribute it and/or modify
* it under the terms of the GNU Lesser General Public License as published
* by the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* ADAPT is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Lesser General Public License for more details.
* 
* You should have received a copy of the GNU Lesser General Public License
* along with ADAPT.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class ShootBall : MonoBehaviour 
{	
	public Camera targetCamera;
	public GameObject ballPrefab;
	public float magnitude;
    public float lifetime = 1.0f;

    private Vector3 lastPosition = Vector3.zero;
    private List<KeyValuePair<GameObject, float>> cleanup;

	void Start () 
    {
        this.cleanup = new List<KeyValuePair<GameObject, float>>();
	}
	
	void Update()
    {
		if (Input.GetKeyDown(KeyCode.F) == true)
        {
            Ray cursorRay = targetCamera.ScreenPointToRay(Input.mousePosition);
            GameObject newBall = 
                Instantiate(
                    ballPrefab, 
                    targetCamera.transform.position, 
                    targetCamera.transform.rotation) 
                as GameObject;
            Vector3 currentPosition = this.targetCamera.transform.position;
            Vector3 velocity = 
                (currentPosition - this.lastPosition) / Time.deltaTime;
			newBall.rigidbody.velocity = 
                velocity + (cursorRay.direction * magnitude);
            this.cleanup.Add(
                new KeyValuePair<GameObject, float>(
                    newBall, 
                    Time.time + this.lifetime));
		}

        foreach (KeyValuePair<GameObject, float> ball in this.cleanup)
            if (ball.Value < Time.time)
                GameObject.Destroy(ball.Key);
        this.cleanup.RemoveAll(n => n.Value < Time.time);
        this.lastPosition = this.targetCamera.transform.position;
	}
}
