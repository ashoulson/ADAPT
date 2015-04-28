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

/*
Copyright (c) 2008, Rune Skovbo Johansen & Unity Technologies ApS

See the document "TERMS OF USE" included in the project folder for licencing details.
*/
#endregion

using UnityEngine;
using System.Collections;

public class GhostOriginal : MonoBehaviour {
	
	public GameObject character;
	public Vector3 offset;
	
	public void Synch() {
		foreach (AnimationState state in character.GetComponent<Animation>()) {
			AnimationState ownState = GetComponent<Animation>()[state.name];
			if (ownState==null) {
				GetComponent<Animation>().AddClip(state.clip, state.name);
				ownState = GetComponent<Animation>()[state.name];
			}
			if (ownState.enabled != state.enabled) {
				ownState.wrapMode = state.wrapMode;
				ownState.enabled = state.enabled;
				ownState.speed = state.speed;
			}
			ownState.weight = state.weight;
			ownState.time = state.time;
		}
	}
	
	void LateUpdate() {
		transform.position = character.transform.position+offset;
		transform.rotation = character.transform.rotation;
	}
}
