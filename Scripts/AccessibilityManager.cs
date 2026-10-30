using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class AccessibilityManager : MonoBehaviour
{
    [Header("Material de Pós-Processamento")]
    [SerializeField] private Material filterMaterial;

    [Header("Componentes de Interface (UI)")]
    [SerializeField] private Toggle filterToggle;
    [SerializeField] private Slider intensitySlider;

    [Header("Outline")]
    [SerializeField] private Toggle outlineToggle;
    [SerializeField] private OutlineManager outlineManager;


    private void Start()
    {

        // FILTRO DE ACESSIBILIDADE

        // Toggle: liga/desliga o filtro
        if (filterToggle != null)
        {
            filterToggle.onValueChanged.AddListener(SetFilterActive);

            SetFilterActive(filterToggle.isOn);
        }

        // Slider: controla a intensidade/direção da correção
        if (intensitySlider != null)
        {
            intensitySlider.onValueChanged.AddListener(SetFilterIntensity);

            SetFilterIntensity(intensitySlider.value);
        }

        // OUTLINE
        if (outlineToggle != null)
        {
            outlineToggle.onValueChanged.AddListener(SetOutlineActive);

            SetOutlineActive(outlineToggle.isOn);
        }
    }


    private void Update()
    {
        // TAB - LIBERA/TRAVA O CURSOR
        if (Keyboard.current != null &&
            Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    public void SetFilterActive(bool isActive)
    {
        if (filterMaterial != null)
        {
            float value = isActive
                ? (intensitySlider != null ? intensitySlider.value : 0.0f)
                : 0.0f;

            filterMaterial.SetFloat("_Severity", value);

        }
        else
        {
            Debug.LogError("FILTER MATERIAL É NULL!");
        }
    }


    // INTENSIDADE / DIREÇÃO DA CORREÇÃO

    public void SetFilterIntensity(float value)
    {
        if (filterMaterial != null &&
            (filterToggle == null || filterToggle.isOn))
        {
            filterMaterial.SetFloat("_Severity", value);

        }
    }


    // OUTLINE

    public void SetOutlineActive(bool isActive)
    {
        if (outlineManager != null)
        {
            outlineManager.SetOutlineActive(isActive);

        }
        else
        {
            Debug.LogError("OUTLINE MANAGER É NULL!");
        }
    }
}