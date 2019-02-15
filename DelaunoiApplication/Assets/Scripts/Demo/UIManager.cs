using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

using Delaunoi;

public class UIManager : MonoBehaviour
{
	[SerializeField]
	private GameObject ToolsCanvas;

    // Generators
    [SerializeField]
    private GameObject GeneratorsWrapper;

	[SerializeField]
	private GuibasStolfiPlane DelaunoiHandler;

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


	// Use this for initialization
	void Start ()
	{
        PrepareToggles();
		PrepareSliders();
        PrepareDropdown();

        // Get values from handler
        InitValues();
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

    /// Avoid Negative and 0
    private char UnsignedIntValidator(string input, int charIndex, char addedChar)
    {
        //Checks if a dollar sign is entered....
        if (addedChar == '-')
        {
            // ... if it is change it to an empty character.
            addedChar = '\0';
        }
        else if(addedChar == '0' || addedChar == '1')
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
            DelaunoiHandler.pointNumber = Convert.ToInt32(nbPointsInput.text);
		}

        if (seedInput.text != String.Empty)
        {
            DelaunoiHandler.seed = Convert.ToInt32(seedInput.text);
        }

        if (HaltonField.text != String.Empty)
        {
            int base1 = Convert.ToInt32(HaltonField.text);
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

        }

        if (MinimalDistance.text != String.Empty)
        {
            DelaunoiHandler.minimalDistance = Convert.ToInt32(MinimalDistance.text);
        }

		// Ask handle to update data
        DelaunoiHandler.UpdateTriangulation();
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
        DelaunoiHandler.celltype = (FaceConfig)change.value;
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

}
