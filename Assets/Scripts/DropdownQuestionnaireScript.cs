using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class DropdownQuestionnaireScript : MonoBehaviour {

    List<string> choices = new List<string>() { "Select a room . . ." };
    public Dropdown dropdown;
    public Text selectedAnswer;

    // ********************************************************************** //

    void Start()
    {
        PopulateList();
    }

    // ********************************************************************** //

    void PopulateList()
    {
        for (int i = 0; i < GameController.control.questionData.answers.Length; i++) 
        { 
            choices.Add(GameController.control.questionData.answers[i].answerText);
        }
        dropdown.AddOptions(choices);
    }

    // ********************************************************************** //

    public void DropdownIndexChanged(int index)
    {
        selectedAnswer.text = choices[index];
        selectedAnswer.color = (index == 0) ? new Color(215f / 255f, 252f / 255f, 255f / 255f, 126f / 255f) : Color.white;
        GameController.control.SetQuestionnaireAnswer(choices[index]);
    }

    // ********************************************************************** //
}
