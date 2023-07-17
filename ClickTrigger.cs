using System;
using System.Collections.Generic;
using UnityEngine;

public class ClickTrigger : MonoBehaviour
{
	TicTacToeAI _ai;

	[SerializeField]
	public int _myCoordX = 0;
	[SerializeField]
	public int _myCoordY = 0;

	public int boardPosition;

	[SerializeField]
	private bool canClick;

	public bool spaceTaken;

	private void Awake()
	{
		_ai = FindObjectOfType<TicTacToeAI>();
	}

	private void Start(){

		_ai.onGameStarted.AddListener(AddReference);
		_ai.onGameStarted.AddListener(() => SetInputEndabled(true));
		_ai.onPlayerWin.AddListener((win) => SetInputEndabled(false));
		_ai.onPlayerChanged.AddListener(() => PlayerChanged());
    }

	private void SetInputEndabled(bool click)
	{
		canClick = click;
	}

	private void PlayerChanged()
	{
        canClick = !canClick;
    }

	private void AddReference()
	{
		_ai.RegisterTransform(_myCoordX, _myCoordY, this);
	//	canClick = true;
	}

	private void OnMouseDown()
	{
		if (spaceTaken == false)
		{
			if (canClick)
			{
				_ai.PlayerSelects(_myCoordX, _myCoordY, boardPosition);
				spaceTaken = true;
			}
		}
		//canClick = false;

	}
}
