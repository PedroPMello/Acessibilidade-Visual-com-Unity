# Remapeamento de Cores e Silhuetas como Recursos de Acessibilidade para Deuteranopia em Ambientes Virtuais Interativos 3D

Protótipo assistivo para **Unity (Universal Render Pipeline)** que visa ajudar pessoas com **deuteranopia** (um tipo de daltonismo) a distinguir elementos de uma cena 3D, usando duas estratégias que se complementam: **mudar as cores da imagem** e **desenhar contornos ao redor dos objetos**.

> Esta é a **versão do artigo** (tag `artigo-unijui-2026`), com suporte apenas a deuteranopia. Uma versão estendida, desenvolvida para o TCC, será publicada neste repositório.

📄 **Artigo:** *Remapeamento de cores e silhuetas como recursos de acessibilidade para deuteranopia em ambientes virtuais interativos 3D*, Pedro Pizzolato Mello e Maikon Cismoski dos Santos. XXXIV Seminário de Iniciação Científica, Salão do Conhecimento UNIJUÍ 2026.
🎮 **Demo (Unity Play):** [https://play.unity.com/en/games/0570218b-f717-41e2-8e45-f51405040eee/prototipo-acessibilidade-visual]

[Comparativo de intensidades do filtro](docs/figura3.png)
[Cena sem e com silhueta](docs/figura4.png)

## Sumário

- [O que é este projeto?](#o-que-é-este-projeto)
- [Tecnologias utilizadas (stack)](#tecnologias-utilizadas-stack)
- [Como funciona](#como-funciona)
- [Estrutura do repositório](#estrutura-do-repositório)
- [Requisitos](#requisitos)
- [Instalação e uso](#instalação-e-uso)
- [Limitações conhecidas](#limitações-conhecidas)
- [Trabalhos futuros](#trabalhos-futuros)
- [Como citar](#como-citar)
- [Créditos e licença](#créditos-e-licença)

---

## O que é este projeto?

### O problema

Em jogos e ambientes virtuais 3D, a cor costuma carregar informação: uma porta verde pode significar "aberta" e uma vermelha, "fechada"; um item verde pode ser coletável e um vermelho, perigoso. Para quem tem **deuteranopia**, esse tipo de pista falha. A deuteranopia é um tipo de daltonismo causado pela ausência funcional dos cones M da retina, e a principal consequência é a dificuldade de **distinguir verde de vermelho**.

Os filtros de daltonismo nativos de muitas ferramentas costumam alterar a estética da cena sem resolver de fato a barreira de acessibilidade. O resultado é que o usuário é excluído de partes da experiência.

### A solução proposta

O protótipo oferece **duas ajudas independentes**, que podem ser usadas separadamente ou juntas:

| Ajuda | O que faz | Para que serve |
|---|---|---|
| **Remapeamento de cores** | Altera as cores da imagem final em tempo real, afastando verdes e vermelhos entre si | Cria contraste entre cores que o usuário confunde |
| **Silhuetas (contornos)** | Desenha uma linha ao redor dos objetos 3D | Permite identificar os objetos pela **forma**, mesmo sem distinguir a cor |

A ideia central é a **redundância visual**: o usuário pode identificar um elemento tanto pela cor quanto pelo contraste de forma. Se uma das ajudas não for suficiente, a outra compensa. O usuário controla a intensidade do filtro, então a cena não precisa ser alterada mais do que o necessário, o que preserva a intenção visual original.

### Visão geral do fluxo

Um **menu 2D de calibração** controla as funções. A partir dele, o usuário liga as silhuetas e o filtro de cores, que roda como um *fragment shader* no pós-processamento. A validação do protótipo foi feita comparando visualmente a cena original com as cenas adaptadas.

```
Interface 2D  →  Pistas Visuais (silhuetas)  →  Fragment Shader (cores)  →  Validação prática
```

---

## Tecnologias utilizadas (stack)

| Tecnologia | Papel no projeto |
|---|---|
| **Unity** | Motor gráfico em que o ambiente 3D e o protótipo foram desenvolvidos |
| **Universal Render Pipeline (URP)** | Pipeline de renderização. É nele que o filtro se encaixa como *Renderer Feature* |
| **C#** | Scripts de controle: `AccessibilityManager`, `OutlineManager`, `OutlineObject` e a *Renderer Feature* do filtro |
| **HLSL / ShaderLab** | Shaders: `ColorBlindnessFilter` (remapeamento de cores) e `Outline` (silhuetas) |
| **Render Graph (URP)** | API usada pelo `ColorBlindnessPass` para inserir o filtro na etapa de pós-processamento |
| **Unity UI (uGUI)** | `Toggle` e `Slider` da interface de calibração |
| **Input System** | Leitura do teclado (tecla Tab) no `AccessibilityManager` |
| **Git / GitHub** | Versionamento. Este repositório contém só código e shaders autorais |

---

## Como funciona

### 1. Remapeamento cromático (`ColorBlindnessFilter.shader`)

O filtro roda em pós-processamento, por meio de uma *Renderer Feature* da URP (`ColorBlindnessFeature` + `ColorBlindnessPass`) que aplica um *blit* com o material do shader sobre a imagem da câmera, antes do pós-processamento padrão.

Dentro do shader, a cor é convertida de sRGB para linear (aproximação com γ = 2,2) e passa por uma **multiplicação de matriz nos canais RGB**. O parâmetro `_Severity` (`i`) controla a mistura:

```
i < 0 :  alpha = -i,  beta = 0
i ≥ 0 :  alpha = 0,   beta = i
gamma = 1 - (alpha + beta)

R' = R
G' = G
B' = alpha·R + beta·G + gamma·B
```

Para `i > 0`, parte da informação do canal **verde** é transferida para o **azul**; para `i < 0`, a do canal **vermelho**. Vermelho e verde são preservados, e apenas o azul muda. Na cena de teste, com `i = 1` as superfícies verdes tendem ao ciano e as azuis escurecem, enquanto os vermelhos mantêm sua intensidade. Esse deslocamento gradual cria contraste entre cores que o observador com deuteranopia tende a confundir.

| `_Severity` | Efeito |
|---|---|
| `0` | Sem alteração (filtro inativo) |
| `0 a 1` | Transferência crescente de verde para azul (**configuração usada para deuteranopia no artigo**) |
| `-1 a 0` | Ramo implementado no shader (transferência de vermelho para azul), mas **não exposto na interface desta versão** (o slider vai de 0 a 1) e **não avaliado** |

### 2. Silhuetas (`Outline.shader`, `OutlineManager`, `OutlineObject`)

Para cada objeto com `MeshFilter` presente na cena ao iniciar, o `OutlineManager` cria um objeto filho com a mesma malha e o material de contorno. O shader usa a técnica de **casco invertido** (*inverted hull*): renderiza as faces de trás (`Cull Front`) da malha, expandida ao longo das normais em espaço do mundo (`_OutlineWidth`), com cor sólida (`_OutlineColor`). A parte que aparece além da borda do objeto original forma a silhueta.

### 3. Controle (`AccessibilityManager`)

Liga os elementos da interface 2D ao material do filtro (`_Severity`) e ao `OutlineManager`. Os detalhes de cada ligação estão na seção abaixo.

---

## Estrutura do repositório

```
acessibilidade-visual-unity/
├── README.md
├── LICENSE
├── Scripts/
│   ├── AccessibilityManager.cs
│   ├── ColorBlindnessFeature.cs
│   ├── ColorBlindnessPass.cs
│   ├── OutlineManager.cs
│   ├── OutlineObject.cs
│   └── (arquivos .meta correspondentes)
└── Shaders/
    ├── ColorBlindnessFilter.shader
    ├── Outline.shader
    └── (arquivos .meta correspondentes)
```

Este repositório contém **apenas o código e os shaders autorais**. Cenas, assets e demais recursos do projeto base do Unity não são redistribuídos aqui.

## Requisitos

- Unity **[6000.5.8f1]**
- Universal Render Pipeline (URP) **[17.5.0]**
- Pacote **Input System**

---

## Instalação e uso

### Passo a passo

1. Copie as pastas `Scripts/` e `Shaders/` deste repositório para dentro do `Assets/` do seu projeto URP, mantendo os arquivos `.meta`.
2. **Materiais:** crie um material com o shader `Custom/ColorBlindnessFilter` e outro com `Custom/Outline`.
3. **Renderer Feature:** no asset do Renderer da URP, clique em *Add Renderer Feature*, escolha `ColorBlindnessFeature` e atribua o material do filtro. Se o projeto tiver mais de um Renderer (por exemplo, os níveis de qualidade *PC* e *Mobile* dos templates da URP), adicione o Feature em **todos** os Renderers usados pelos níveis de qualidade da plataforma de destino. Caso contrário, o filtro só funciona onde foi adicionado. Isso é comum de passar despercebido: o Editor pode usar um nível de qualidade e o build WebGL, outro.
4. **Interface:** crie um Canvas com os três controles descritos na tabela abaixo.
5. **Cena:** crie um GameObject com `OutlineManager` e outro com `AccessibilityManager`, e preencha os campos do Inspector conforme as tabelas abaixo.
6. Execute a cena. Use **Tab** para liberar o cursor e interagir com a interface.

> Use **o mesmo material** do filtro no *Renderer Feature* e no `AccessibilityManager`. Se forem materiais diferentes, o slider não terá efeito visível.

### Controles da interface e o que cada um aciona

Os controles são criados como componentes padrão da Unity UI. O `AccessibilityManager` registra os eventos por código (`AddListener`), então **não é necessário** ligar `OnValueChanged` manualmente no Inspector.

| Controle (texto na interface) | Componente | Campo no `AccessibilityManager` | Método acionado | O que altera |
|---|---|---|---|---|
| **Ativar Filtro** | `Toggle` | `filterToggle` | `SetFilterActive(bool)` | Liga/desliga o filtro: define `_Severity` com o valor do slider (ligado) ou `0` (desligado) |
| **Slider de intensidade** | `Slider` (Min `0`, Max `1`) | `intensitySlider` | `SetFilterIntensity(float)` | Define `_Severity` em tempo real, apenas quando o filtro está ativo |
| **Realçar contornos** | `Toggle` | `outlineToggle` | `SetOutlineActive(bool)` | Chama `OutlineManager.SetOutlineActive(bool)`, que mostra ou oculta os contornos de todos os objetos |

### Campos a preencher no Inspector

| Componente | Campo | O que atribuir |
|---|---|---|
| `AccessibilityManager` | `filterMaterial` | Material com o shader `Custom/ColorBlindnessFilter` (o mesmo do Renderer Feature) |
| `AccessibilityManager` | `filterToggle` | Toggle "Ativar Filtro" |
| `AccessibilityManager` | `intensitySlider` | Slider de intensidade |
| `AccessibilityManager` | `outlineToggle` | Toggle "Realçar contornos" |
| `AccessibilityManager` | `outlineManager` | O GameObject que contém o `OutlineManager` |
| `OutlineManager` | `outlineMaterial` | Material com o shader `Custom/Outline` |
| `ColorBlindnessFeature` | `settings.material` | Material do filtro |

### Propriedades dos shaders

| Shader | Propriedade | Tipo / faixa | Padrão | Função |
|---|---|---|---|---|
| `Custom/ColorBlindnessFilter` | `_Severity` | `Range(-1, 1)` | `0` | Intensidade e direção do remapeamento (veja a tabela de `_Severity` acima). **Controlada pelo código** |
| `Custom/Outline` | `_OutlineColor` | `Color` | preto `(0,0,0,1)` | Cor da silhueta. Ajustada no Inspector do material |
| `Custom/Outline` | `_OutlineWidth` | `Range(0, 0.05)` | `0.01` | Espessura da silhueta, em unidades do mundo. Ajustada no Inspector do material |

---

## Limitações conhecidas

- Apenas **deuteranopia** foi avaliada nesta versão.
- Os contornos são criados na inicialização e só cobrem objetos ativos com `MeshFilter`. Objetos criados depois, inativos no início ou com `SkinnedMeshRenderer` não recebem contorno.
- A espessura do contorno é definida em unidades do mundo, então varia com a distância da câmera.
- Em malhas com arestas duras (como cubos com normais separadas por face), o casco invertido pode apresentar pequenas descontinuidades nos cantos.
- Os objetos de contorno usam as configurações padrão de renderização, então também **projetam e recebem sombras**.
- O estado inicial do toggle de contorno pode ficar dessincronizado: se ele começar **ligado** na cena, a ordem de execução do `Start` entre `AccessibilityManager` e `OutlineManager` não é garantida, e os contornos podem iniciar desligados. Com o toggle iniciando desligado, o comportamento é o esperado.
- O valor de `_Severity` fica gravado no asset do material depois de sair do Play Mode (no Editor). Convém zerá-lo antes de uma nova execução.
- A validação foi **visual e comparativa**, sem testes com usuários.

## Trabalhos futuros

- Outros tipos de daltonismo: protanopia, tritanopia e monocromacia.
- Ajustes pontuais nas ferramentas implementadas e na UI.
- Aprimoramento do ambiente virtual 3D.
- Testes empíricos com usuários daltônicos e tricromatas, para avaliar eficácia, usabilidade e aceitação.

## Como citar

```bibtex
@inproceedings{mello2026remapeamento,
  author    = {Mello, Pedro Pizzolato and Santos, Maikon Cismoski dos},
  title     = {Remapeamento de cores e silhuetas como recursos de acessibilidade para deuteranopia em ambientes virtuais interativos 3D},
  booktitle = {XXXIV Semin{\'a}rio de Inicia{\c c}{\~a}o Cient{\'\i}fica, Sal{\~a}o do Conhecimento UNIJU{\'I}},
  year      = {2026}
}
```

### Referências principais

- RASKAR, R.; COHEN, M. *Image precision silhouette edges*. Proceedings of the 1999 Symposium on Interactive 3D Graphics, 1999.
- ANDRADE, R. et al. *Playing blind: Revealing the world of gamers with visual impairment*. CHI 2019.
- BROWN, M.; ANDERSON, S. L. *Designing for disability: Evaluating the state of accessibility design in video games*. Games and Culture, 2020.
- MONTEIRO, F. d. L. V. et al. *Jogos digitais e usuários com daltonismo: como a acessibilidade pode afetar a jogabilidade*. SBGames 2022.
- CARNEIRO, M. P. d. G. *Simulações de daltonismo no desenvolvimento de jogos cromaticamente acessíveis para dispositivos móveis*. UFC, 2024.

## Créditos

- Autor: Pedro Pizzolato Mello. Orientador: Maikon Cismoski dos Santos.
- Pesquisa desenvolvida no IFSul, Câmpus Passo Fundo.
- Projeto estruturado a partir do tutorial **[Get Started with Unity: In-Editor Tutorial]** ([https://learn.unity.com/tutorial/get-started-with-unity-in-editor-tutorial]). Cena e assets do projeto base pertencem à Unity Technologies e não são distribuídos neste repositório.
