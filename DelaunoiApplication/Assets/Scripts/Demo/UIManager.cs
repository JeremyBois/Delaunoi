using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using Delaunoi;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GuibasStolfiPlane DelaunoiHandler;


	[SerializeField]
	private GameObject ToolsPanel;

    [SerializeField]
    private GameObject InfosPanel;

    [SerializeField]
    private GameObject InfosButton;

    [SerializeField]
    private GameObject GameButton;

    // Generators
    [SerializeField]
    private GameObject GeneratorsWrapper;



	// Toggles
    public Toggle VorPts;
    public Toggle VorLines;
    public Toggle VorFaces;
    public Toggle DelPts;
    public Toggle DelLines;
    public Toggle DelFaces;

	// Sliders
    public Slider ShapesSlider;
    public Slider LinesSlider;

	// Dropdown
	public Dropdown GeneratorTypeDrop;
    public Dropdown FaceTypeDrop;


	// Input fields
	public InputField nbPointsInput;
    public InputField seedInput;
    public InputField HaltonField;
    public InputField MinimalDistance;

    // Dirty
    bool triangulationIsDirty;
    bool cellIsDirty;

    // FSM
    private enum UIState
    {
        Game,
        Infos
    }
    UIState currentState;


	// Use this for initialization
	void Start ()
	{
        PrepareToggles();
		PrepareSliders();
        PrepareDropdown();

        // Get values from handler
        InitValues();

        // Needed to compute first time
        triangulationIsDirty = true;
        RegenerateDiagram();

        // First show informations
        HideInfos();
	}

    bool Coprime(int value1, int value2)
    {
        while (value1 != 0 && value2 != 0)
        {
            if (value1 > value2)
                value1 %= value2;
            else
                value2 %= value1;
        }
        return Math.Max(value1, value2) == 1;
    }

	void InitValues()
	{
        // Sliders
        LinesSlider.value = DelaunoiHandler.lineScale;
        ShapesSlider.value = DelaunoiHandler.scale;

        // Toggles
        VorPts.isOn = DelaunoiHandler.V_points;
        VorLines.isOn = DelaunoiHandler.V_lines;
        VorFaces.isOn = DelaunoiHandler.V_faces;
        DelPts.isOn = DelaunoiHandler.D_points;
        DelLines.isOn = DelaunoiHandler.D_lines;
        DelFaces.isOn = DelaunoiHandler.D_faces;

        // Dropdown
        GeneratorTypeDrop.value = (int)DelaunoiHandler.usedGenerator;
        FaceTypeDrop.value = (int)DelaunoiHandler.celltype;

        // Input fields
        seedInput.text = DelaunoiHandler.seed.ToString();
        nbPointsInput.text = DelaunoiHandler.pointNumber.ToString();
        HaltonField.text = DelaunoiHandler.bases[0].ToString();
        MinimalDistance.text = DelaunoiHandler.minimalDistance.ToString();

        nbPointsInput.onValidateInput += delegate (string input, int charIndex, char addedChar)
            {
                return UnsignedIntValidator(input, charIndex, addedChar);
            };
        seedInput.onValidateInput += delegate (string input, int charIndex, char addedChar)
            {
                return UnsignedIntValidator(input, charIndex, addedChar);
            };
        HaltonField.onValidateInput += delegate (string input, int charIndex, char addedChar)
            {
                return UnsignedIntValidator(input, charIndex, addedChar);
            };
        MinimalDistance.onValidateInput += delegate (string input, int charIndex, char addedChar)
            {
                return UnsignedIntValidator(input, charIndex, addedChar);
            };
    }

    /// Only integers are valid
    private char UnsignedIntValidator(string input, int charIndex, char addedChar)
    {
        //Checks if a dollar sign is entered....
        if (!Char.IsNumber(addedChar))
        {
            // ... if it is change it to an empty character.
            addedChar = '\0';
        }
        else if(addedChar == '0')
        {
            if (input == string.Empty)
            {
                addedChar = '\0';
            }
        }
        return addedChar;
    }

    private char BaseValidator(string input, int charIndex, char addedChar)
    {
        //Checks if a dollar sign is entered....
        if (!Char.IsNumber(addedChar))
        {
            // ... if it is change it to an empty character.
            addedChar = '\0';
        }
        else if (addedChar == '0')
        {
            if (input == string.Empty)
            {
                addedChar = '\0';
            }
        }

        return addedChar;
    }


	public void RegenerateDiagram()
	{
		if (nbPointsInput.text != String.Empty)
		{
            int newNumber = Convert.ToInt32(nbPointsInput.text);
            if (newNumber < 3)
            {
                nbPointsInput.text = DelaunoiHandler.pointNumber.ToString();
            }
            else if (newNumber != DelaunoiHandler.pointNumber)
            {
                DelaunoiHandler.pointNumber = newNumber;
                triangulationIsDirty = true;
            }
		}

        if (seedInput.text != String.Empty)
        {
            int newSeed = Convert.ToInt32(seedInput.text);
            if (newSeed != DelaunoiHandler.seed)
            {
                DelaunoiHandler.seed = newSeed;
                triangulationIsDirty = true;
            }
        }

        if (HaltonField.text != String.Empty)
        {
            int base1 = Convert.ToInt32(HaltonField.text);
            if (base1 < 2)
            {
                HaltonField.text = DelaunoiHandler.bases[0].ToString();
            }
            else if (base1 > 33)
            {
                HaltonField.text = DelaunoiHandler.bases[0].ToString();
            }
            else if (DelaunoiHandler.bases[0] != base1)
            {
                int base2 = base1 + 1;

                // Find closest coprime
                int maxTest = 1;
                while (!Coprime(base1, base2))
                {
                    ++base2;
                    ++maxTest;
                }

                if (maxTest < 10)
                {
                    Debug.Log(base2);
                    DelaunoiHandler.bases[0] = base1;
                    DelaunoiHandler.bases[1] = base2;
                }
                triangulationIsDirty = true;
            }
        }

        if (MinimalDistance.text != String.Empty)
        {
            int distance = Convert.ToInt32(MinimalDistance.text);
            if (distance <= 0)
            {
                MinimalDistance.text = DelaunoiHandler.minimalDistance.ToString();
            }
            else if (distance != DelaunoiHandler.minimalDistance)
            {
                DelaunoiHandler.minimalDistance = distance;
                triangulationIsDirty = true;
            }
        }

        // Cell
        FaceConfig conf = (FaceConfig)FaceTypeDrop.value;
        if (DelaunoiHandler.celltype != conf)
        {
            DelaunoiHandler.celltype = conf;
            cellIsDirty = true;
        }

        // Generator
        GeneratorType gen = (GeneratorType)GeneratorTypeDrop.value;
        if (gen != DelaunoiHandler.usedGenerator)
        {
            DelaunoiHandler.usedGenerator = gen;
            triangulationIsDirty = true;
        }

		// Ask handle to update data
        DelaunoiHandler.UpdateTriangulation(triangulationIsDirty, cellIsDirty);
        triangulationIsDirty = false;
        cellIsDirty = false;
	}

	public void UpdateVoronoiPts(Toggle change)
	{
        DelaunoiHandler.V_points = change.isOn;
	}

    public void UpdateVoronoiLines(Toggle change)
    {
        DelaunoiHandler.V_lines = change.isOn;
    }

    public void UpdateVoronoiFaces(Toggle change)
    {
        DelaunoiHandler.V_faces = change.isOn;
    }

    public void UpdateDelaunayPts(Toggle change)
    {
        DelaunoiHandler.D_points = change.isOn;
    }

    public void UpdateDelaunayLines(Toggle change)
    {
        DelaunoiHandler.D_lines = change.isOn;
    }

    public void UpdateDelaunayFaces(Toggle change)
    {
        DelaunoiHandler.D_faces = change.isOn;
    }

    public void UpdateShapeSlider(Slider change)
    {
        DelaunoiHandler.scale = change.value;
    }

    public void UpdateLineSlider(Slider change)
    {
        DelaunoiHandler.lineScale = change.value;
    }

    public void UpdateFaceType(Dropdown change)
    {
        FaceConfig conf = (FaceConfig)change.value;
        if (DelaunoiHandler.celltype != conf)
        {
            DelaunoiHandler.celltype = conf;
            cellIsDirty = true;
        }
    }

    public void UpdateGenerator(Dropdown change)
    {
        switch (change.value)
        {
            case 0:
                GeneratorsWrapper.transform.GetChild(1).gameObject.SetActive(true);
                GeneratorsWrapper.transform.GetChild(2).gameObject.SetActive(false);
                DelaunoiHandler.usedGenerator = GeneratorType.Halton;
                break;
            case 1:
                GeneratorsWrapper.transform.GetChild(1).gameObject.SetActive(false);
                GeneratorsWrapper.transform.GetChild(2).gameObject.SetActive(true);
                DelaunoiHandler.usedGenerator = GeneratorType.Poisson;
                break;
            default:
                break;
        }

        triangulationIsDirty = true;
    }


	void PrepareDropdown()
	{
        FaceTypeDrop.onValueChanged.AddListener(delegate
        {
            UpdateFaceType(FaceTypeDrop);
        });

        GeneratorTypeDrop.onValueChanged.AddListener(delegate
        {
            UpdateGenerator(GeneratorTypeDrop);
        });
	}



	void PrepareSliders()
	{
        ShapesSlider.onValueChanged.AddListener(delegate
        {
            UpdateShapeSlider(ShapesSlider);
        });

        LinesSlider.onValueChanged.AddListener(delegate
        {
            UpdateLineSlider(LinesSlider);
        });
	}


	void PrepareToggles()
	{
        VorPts.onValueChanged.AddListener(delegate
        {
            UpdateVoronoiPts(VorPts);
        });

        VorLines.onValueChanged.AddListener(delegate
        {
            UpdateVoronoiLines(VorLines);
        });

        VorFaces.onValueChanged.AddListener(delegate
        {
            UpdateVoronoiFaces(VorFaces);
        });

        DelPts.onValueChanged.AddListener(delegate
        {
            UpdateDelaunayPts(DelPts);
        });

        DelLines.onValueChanged.AddListener(delegate
        {
            UpdateDelaunayLines(DelLines);
        });

        DelFaces.onValueChanged.AddListener(delegate
        {
            UpdateDelaunayFaces(DelFaces);
        });
	}

    public void Exit()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }

    public void ShowInfos()
    {
        SetState(UIState.Infos);
    }

    public void HideInfos()
    {
        SetState(UIState.Game);
    }

    private void SetState(UIState newState)
    {
        switch (newState)
        {
            case UIState.Infos:
                ToolsPanel.SetActive(false);
                InfosPanel.SetActive(true);
                InfosButton.SetActive(false);
                GameButton.SetActive(true);
                break;
            case UIState.Game:
                ToolsPanel.SetActive(true);
                InfosPanel.SetActive(false);
                InfosButton.SetActive(true);
                GameButton.SetActive(false);
                break;
        }

        currentState = newState;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            RegenerateDiagram();
        }
    }

}
