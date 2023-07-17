//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum TicTacToeState{none, cross, circle}

[System.Serializable]
public class WinnerEvent : UnityEvent<int>
{
}

public class TicTacToeAI : MonoBehaviour
{

    int _aiLevel;

	TicTacToeState[,] boardState;

	[SerializeField]
	private bool _isPlayerTurn;

	[SerializeField]
	private int _gridSize = 3;

	[SerializeField]
	private TicTacToeState playerState = TicTacToeState.cross;
	TicTacToeState aiState = TicTacToeState.circle;

	[SerializeField]
	private GameObject _xPrefab;

	[SerializeField]
	private GameObject _oPrefab;

	public UnityEvent onGameStarted;

	//Call This event with the player number to denote the winner
	public WinnerEvent onPlayerWin;
	bool gameOver = false;

	public UnityEvent onPlayerChanged;

	ClickTrigger[,] _triggers;
    //available positions for the ai to choose from
	[SerializeField] List<int> availablePositions = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    //prefered positions based on the players previous move
	[SerializeField] List<int> preferedPositions = new List<int>();
    //these are the actual trigger objects to their transforms can be grabbed
	[SerializeField] List<GameObject> spawnPositions;

	private int playerPoint = 1;
	private int aiPoint;

    //lists of each row for calculating if a player is about to win lists made visible in inspector for ease of debugging
    [SerializeField] List<int> rowOneSpots = new List<int> { 1, 2, 3 };
    [SerializeField] List<int> rowTwoSpots = new List<int> { 4, 5, 6 };
    [SerializeField] List<int> rowThreeSpots = new List<int> { 7, 8, 9 };
    [SerializeField] List<int> columnOneSpots = new List<int> { 1, 4, 7 };
    [SerializeField] List<int> columnTwoSpots = new List<int> { 2, 5, 8 };
    [SerializeField] List<int> columnThreeSpots = new List<int> { 3, 6, 9};
    [SerializeField] List<int> diagonalOneSpots = new List<int> { 1, 5, 9 };
    [SerializeField] List<int> diagonalTwoSpots = new List<int> { 3, 5, 7 };

    //tallys the score of each solution
     int rowOne;
	 int rowTwo;
	 int rowThree;
     int columnOne;
	 int columnTwo;
	 int columnThree;
	 int diagonalOne;
	public int diagonalTwo;

    //check as in check from chess, sees if a player is close to getting 3 in a row
    bool checkMove = false;

    //a list of all possible winning solutions it is re evaluated each turn
    private List<int> solutions = new List<int>();

	private void Awake()
	{
		if (onPlayerWin == null) {
			onPlayerWin = new WinnerEvent();
		}

		PopulateSolutions();

    }

    public void StartAI(int AILevel) {
		_aiLevel = AILevel;
		StartGame();
	}

	public void RegisterTransform(int myCoordX, int myCoordY, ClickTrigger clickTrigger)
	{
		_triggers[myCoordX, myCoordY] = clickTrigger;
	}

	private void StartGame()
	{
		//Debug.Log("game started");
		_triggers = new ClickTrigger[3, 3];
		onGameStarted.Invoke();
	}

    // boardPosition was added to each trigger space so they could be tallyed and tracked numarically 1-9
	public void PlayerSelects(int coordX, int coordY, int boardPosition) {
	
		SetVisual(coordX, coordY, playerState);
		CalculateAvailablePositions(boardPosition, playerState);
	}

	public void AiSelects(int coordX, int coordY, int boardPosition) {

		SetVisual(coordX, coordY, aiState);
        CalculateAvailablePositions(boardPosition, aiState);
    }

	private void SetVisual(int coordX, int coordY, TicTacToeState targetState)
	{
		WhosTurnIsIt();

        Instantiate(
			targetState == TicTacToeState.circle ? _oPrefab : _xPrefab,
			_triggers[coordX, coordY].transform.position,
			Quaternion.identity
		);

    }

	//stops a player from being able to quick click on spaces
	private void WhosTurnIsIt()
	{
		_isPlayerTurn = !_isPlayerTurn;
		onPlayerChanged.Invoke();
    }

    //this is the meat of the code, triggers AI responces based upon player input
    private void CalculateAvailablePositions(int boardPosition, TicTacToeState targetState)
	{
        //if a point is added to the row it counts towards the player
		if(targetState == playerState)
		{
			playerPoint = 1;
	
        }
        //if a point is subtracted it counts towards the ai
		if(targetState == aiState)
		{
			playerPoint = -1;
		}

        //gets the position that the player selected and provides the ai with choices based on the players selection
		switch (boardPosition)
		{
			//row One///////////////////////
			case 1:        
				rowOne += playerPoint;
                columnOne += playerPoint;
                diagonalOne += playerPoint;
                //if the player selects hard the ai has a set of prefered moves to chose from
				if(_aiLevel == 1)
				{
                    //removed positions trigger the ai to look for rows with a single space left
                    rowOneSpots.Remove(boardPosition);
                    columnOneSpots.Remove(boardPosition);
                    
					preferedPositions.Clear();
					preferedPositions.AddRange(new int[] {  3, 5, 7, 9 });

                    CheckingForWinningRow(rowOneSpots, columnOneSpots, diagonalOneSpots, diagonalTwoSpots, preferedPositions);

                }
                break;
            case 2:
                rowOne += playerPoint;
                columnTwo += playerPoint;
                if (_aiLevel == 1)
                {

                    rowOneSpots.Remove(boardPosition);
                    columnTwoSpots.Remove(boardPosition);

                    preferedPositions.Clear();
                    preferedPositions.AddRange(new int[] { 1, 3, 5, 8 });
                    CheckingForWinningRow(rowOneSpots, columnTwoSpots, diagonalOneSpots, diagonalTwoSpots, preferedPositions);

                    DebugPositions(1);
                }
                break;
            case 3:
                rowOne += playerPoint;
                columnThree += playerPoint;
                diagonalTwo += playerPoint;
                if (_aiLevel == 1)
                {

                    rowOneSpots.Remove(boardPosition);
                    columnThreeSpots.Remove(boardPosition);
                    diagonalTwoSpots.Remove(boardPosition);

                    preferedPositions.Clear();
                    preferedPositions.AddRange(new int[] { 1, 5, 7, 9 });
                    CheckingForWinningRow(rowOneSpots, columnThreeSpots, diagonalOneSpots, diagonalTwoSpots, preferedPositions);

                    DebugPositions(1);
                }
                break;

				//row two////////////////////////////////
            case 4:
                rowTwo += playerPoint;
                columnOne += playerPoint;
                if (_aiLevel == 1)
                {

                    rowTwoSpots.Remove(boardPosition);
                    columnOneSpots.Remove(boardPosition);

                    preferedPositions.Clear();
                    preferedPositions.AddRange(new int[] { 1, 5, 6, 7 });
                    CheckingForWinningRow(rowTwoSpots, columnOneSpots, diagonalOneSpots, diagonalTwoSpots, preferedPositions);
                    DebugPositions(1);
                }
                break;
            case 5:
                rowTwo += playerPoint;
                columnTwo += playerPoint;
                diagonalOne += playerPoint;
                diagonalTwo += playerPoint;

                if (_aiLevel == 1)
                {

                    rowTwoSpots.Remove(boardPosition);
                    columnTwoSpots.Remove(boardPosition);
                    diagonalOneSpots.Remove(boardPosition);
                    diagonalTwoSpots.Remove(boardPosition);

                    preferedPositions.Clear();
                    preferedPositions.AddRange(new int[] { 1, 3, 7, 9 });

                    CheckingForWinningRow(rowTwoSpots, columnTwoSpots, diagonalOneSpots, diagonalTwoSpots, preferedPositions);
                    DebugPositions(1);
                }
                break;
            case 6:
                rowTwo += playerPoint;
                columnThree += playerPoint;
                if (_aiLevel == 1)
                {

                    rowTwoSpots.Remove(boardPosition);
                    columnThreeSpots.Remove(boardPosition);

                    preferedPositions.Clear();
                    preferedPositions.AddRange(new int[] { 3, 4, 5, 9 });
                    CheckingForWinningRow(rowTwoSpots, columnThreeSpots, diagonalOneSpots, diagonalTwoSpots, preferedPositions);
                    DebugPositions(1);
                }
                break;

				//row Three////////////////////////////////////
            case 7:
                rowThree += playerPoint;
                columnOne += playerPoint;
                diagonalTwo += playerPoint;
                if (_aiLevel == 1)
                {

                    rowThreeSpots.Remove(boardPosition);
                    columnOneSpots.Remove(boardPosition);
                    diagonalTwoSpots.Remove(boardPosition);

                    preferedPositions.Clear();
                    preferedPositions.AddRange(new int[] { 1, 3, 5, 9 });
                    CheckingForWinningRow(rowThreeSpots, columnOneSpots, diagonalOneSpots, diagonalTwoSpots, preferedPositions);
                    DebugPositions(1);
                }
                break;
            case 8:
                rowThree += playerPoint;
                columnTwo += playerPoint;               
                if (_aiLevel == 1)
                {
                   
                        rowThreeSpots.Remove(boardPosition);
                        columnTwoSpots.Remove(boardPosition);
                    
                    preferedPositions.Clear();
                    preferedPositions.AddRange(new int[] { 2, 5, 7, 9 });
                    CheckingForWinningRow(rowThreeSpots, columnTwoSpots, diagonalOneSpots, diagonalTwoSpots, preferedPositions);
                    DebugPositions(1);
                }
                break;
            case 9:
                rowThree += playerPoint;
                columnThree += playerPoint;
                diagonalOne += playerPoint;
                if (_aiLevel == 1)
                {

                    rowThreeSpots.Remove(boardPosition);
                    columnThreeSpots.Remove(boardPosition);
                    diagonalOneSpots.Remove(boardPosition);

                    preferedPositions.Clear();
                    preferedPositions.AddRange(new int[] { 1, 3, 5, 7, });
                    CheckingForWinningRow(rowThreeSpots, columnThreeSpots, diagonalOneSpots, diagonalTwoSpots, preferedPositions);
                    DebugPositions(1);
                }
                break;

        }

        //removes the played position from possible future moves
       availablePositions.Remove(boardPosition);

        if(_aiLevel == 0)
        {
            StartCoroutine(AiThinking(0));
        }
        //adds the new row counts to possible solutions to be counted
		PopulateSolutions();     
    }

    //this checks to see if any of the rows/columns only have one spot int them and forces the ai to pick that spot(sometimes)
    public void CheckingForWinningRow(List<int> row, List<int> column, List<int>diagonalOne, List<int> diagonalTwo, List<int> prefered)
    {
        int checkSpot = 0;
        if (row.Count > 1 && row.Count != 0 &&
            column.Count > 1 && column.Count != 0)
        {
            checkSpot = Random.Range(0, prefered.Count);
            Debug.Log("full moves");
        }
        if (row.Count == 1)
        {
            checkSpot = row[0];
            checkMove = true;
            Debug.Log("row = " + checkSpot);
        }

        if(column.Count == 1)
        {
            checkSpot = column[0];
            checkMove = true;
            Debug.Log("column =" + checkSpot);
        }

        if (_aiLevel == 1)
        {
            if (_isPlayerTurn)
            {
                StartCoroutine(AiThinking(checkSpot));
            }
        }
    }

    //a simple delay to make it seem like the ai is thinking
    IEnumerator AiThinking(int checkSpot)
    {    
        yield return new WaitForSeconds(1.5f);
        RunAiThoughtProcess(checkSpot);  
       
    }

    //where the ai takes impute from the players selection and makes its own choices
	private void RunAiThoughtProcess(int checkpoint)
	{
        
        int position = 0;
        int preferedPosition = 0;

        //randomly selects a position from remaining positions on the board for easy level
        if (_aiLevel == 0)
        {
            Debug.Log("AI=0");
            position = Random.Range(0, availablePositions.Count);
            Debug.Log("Position" + position);

            for (int i = 0; i < availablePositions.Count; i++)
            {
                
                if (availablePositions.Contains(position))
                {
                    //gets the clicktrigger script and uses it to get its number and position for instantiation the -1 accounts for a list going starting from 0 not 1 
                    ClickTrigger aiPositon = spawnPositions[availablePositions[position] - 1].GetComponent<ClickTrigger>(); 
                    AiSelects(aiPositon._myCoordX, aiPositon._myCoordY, aiPositon.boardPosition);
                    availablePositions.Remove(availablePositions[position] - 1);
                    StopAllCoroutines();
                    return;
                }
                else
                {
                    //if the initial prefered position isnt available a second position is chosen       
                    position = Random.Range(0, availablePositions.Count);
                    Debug.Log("alternate pos selected" + position);
                    
                }
            }
        }

        // for the hard version it itterates through possible positions and compares them to prefered positions based on a users input
        if (_aiLevel == 1)
        {
            Debug.Log("AI=1");
            for (int i = 0; i < availablePositions.Count; i++)
            {
                if (checkMove == false)
                {
                    preferedPosition = Random.Range(1, preferedPositions.Count);
                    Debug.Log(("prefereposition" + preferedPosition));
                    position = preferedPositions[preferedPosition];
                    Debug.Log("position = " + position);
                }
                if(checkMove== true)
                {
                    position = checkpoint;
                    Debug.Log(("position" + position));
                }

                if (availablePositions.Contains(position))
                {
                    int newPos = availablePositions.IndexOf(position);
                    Debug.Log("newPos" + newPos);

                    ClickTrigger aiPositon = spawnPositions[availablePositions[newPos] - 1].GetComponent<ClickTrigger>();
                    AiSelects(aiPositon._myCoordX, aiPositon._myCoordY, aiPositon.boardPosition);
                    //keeps the last spot from throwing null
                    if(availablePositions.Count > 1) availablePositions.Remove(availablePositions[newPos] - 1);
                    checkMove = false;
                    return;
                }
                else
                {
                    //if the initial prefered position isnt available a second position is chosen
                    preferedPosition = Random.Range(0, preferedPositions.Count);
                    position = preferedPositions[preferedPosition];
                    Debug.Log("alternate pos selected" + position);
                   
                }
            }
        }
        
	}

    //re evaluates the ints in solutions
	private void PopulateSolutions()
	{		
		solutions.Clear();

        solutions.Add(diagonalOne);
        solutions.Add(diagonalTwo);
        solutions.Add(rowOne);
        solutions.Add(rowTwo);
        solutions.Add(rowThree);
        solutions.Add(columnOne);
        solutions.Add(columnTwo);
        solutions.Add(columnThree);
       
        CalculateWinner();
    }

    //checks to see if any of the solutions have been satisfied and if so who won
    private void CalculateWinner()
    {
         DebugPositions(0);
        for (int i = 0; i < solutions.Count; i++)
        {          
            if (solutions[i] == 3)
            {
                //player wins
                onPlayerWin.Invoke(0);
                //  Debug.Log("Player won");
                StopAllCoroutines();
                gameOver = true;
            }
            if (solutions[i] == -3)
            {
                //ai wins
                onPlayerWin.Invoke(1);
                //   Debug.Log("AI Won");
                gameOver = true;
            }
            if (solutions[i] != -3 && solutions[i] != 3 && availablePositions.Count == 0 && gameOver == false)
            {
                //tie
                StopAllCoroutines();
                onPlayerWin.Invoke(-1);
                gameOver = true;
            }

        }
    }

    //used to check and see available/prefered/solutions positions when switched
    private void DebugPositions(int debug)
    {
        string result = "";
        switch (debug)
        {
            case 0:
                foreach (int num in solutions)
                {
                    result += num;
                   
                }
                Debug.Log("solutions:" + result.ToString() + ", ");
                break;
            case 1:
                foreach (int num in preferedPositions)
                {
                    result += num;
                    
                }
                Debug.Log("prefered positions:" + result.ToString() + ", ");
                break;
            case 2:
                foreach (int num in availablePositions)
                {
                    result += num;
                    
                }
                Debug.Log("available positions:" + result.ToString() + ", ");
                break;

        }
    }
}
