1. High Concept
Um simulador de gerenciamento de guilda medieval onde o jogador constrói a base, recruta heróis descartáveis e gerencia o ciclo de vida das Dungeons: primeiro como ameaças a serem conquistadas, depois como minas de recursos a serem exploradas até a exaustão.

2. Pilares de Design
Heróis são Investimentos: Eles têm permadeath. Perder um herói veterano dói financeiramente e estrategicamente.

O Ciclo da Dungeon: Combate (Risco) -> Mineração (Recompensa Passiva) -> Exaustão (Fim).

Estética Funcional: A customização da base (móveis/decoração) impacta diretamente os status (Buffs passivos).

3. Mecânicas Core
A. O Hub (A Guilda)
Sistema: Grid 2D (Tilemap).

Interação: O jogador coloca objetos em "slots".

Lógica: Objetos emitem buffs globais ou de área.

Cama: Recupera HP/Stress dos heróis ociosos.

Mesa de Estratégia: Aumenta XP ganho em missões.

Decoração: Diminui ganho de stress geral.

B. Os Aventureiros (Assets)
Atributos: Força (Dano), Vitalidade (HP), Velocidade (Tempo de missão).

Stress/Humor:

Stress 100% = "Burnout" (Não trabalha/Sai da guilda).

Recuperação: Precisa estar na base com recursos (Comida/Cama).

Traits (Traços):

Positivos: Robusto (+HP), Ganancioso (+Ouro no loot).

Negativos: Covarde (Foge se HP < 20%), Bêbado (Gasta ouro da guilda).

C. As Dungeons (O Loop Duplo)
O estado da Dungeon muda com o tempo:

Fase 1: Ameaça (Ativa): Requer Heróis de Combate. Tem HP e Dano.

Ação: Enviar Party.

Resultado: Dano nos heróis, XP, Loot inicial.

Vitória: Quando o "Boss" morre, a fase muda.

Fase 2: Mina (Passiva): Requer Trabalhadores (Mineradores).

Ação: Enviar Mineradores (Unidades baratas/fracas).

Resultado: Gera recursos (Ferro/Pedra) por segundo (Idle).

Fim: A mina tem um total de recursos (ex: 5000 minérios). Quando zera, o local desaparece do mapa.

🚀 Roteiro do MVP (Minimum Viable Product)
Como você tem urgência e é um dev solo usando MonoGame, não tente fazer tudo de uma vez. Siga esta ordem estrita para garantir que o jogo seja jogável o mais rápido possível.

Fase 1: A Lógica "Invisible" (Sem Gráficos)
Objetivo: Validar a matemática e o loop de recursos.

Estrutura de Dados: Criar classes Hero, Dungeon, ResourceBank.

Gerador de Dungeons: Criar método que gera uma dungeon com Dificuldade, HP_Inimigo e Riqueza_Minerio.

Sistema de Combate (Simulação): Método ProcessMission(Party party, Dungeon dungeon).

Calcula dano recebido vs dano causado.

Reduz HP dos heróis.

Retorna Loot.

Conversão: Lógica que transforma uma Dungeon (HP = 0) em uma Mina.

Fase 2: A Base Visual (MonoGame Basic)
Objetivo: Ter algo para olhar e clicar.

Grid System: Renderizar uma matriz 10x10 de tiles (chão de madeira).

UI Básica: Criar botões (Rectangles) para "Recrutar", "Missões", "Construir".

Renderização de Entidades: Desenhar os heróis como ícones/sprites parados em cima do Grid.

Nota: Não faça pathfinding agora. Apenas desenhe eles em posições aleatórias válidas ou designadas.

Feedback Visual: Barras de HP e Stress em cima da cabeça dos heróis.

Fase 3: O Loop Completo (Jogável)
Objetivo: O jogo diverte e fecha o ciclo.

Menu de Expedição: Uma janela (Pop-up) que lista as dungeons. O jogador clica na dungeon -> clica nos heróis -> botão "Enviar".

Timer System: Implementar o tempo real. Heróis somem da base (estado OnMission) e voltam após X segundos.

Mineração: Implementar a UI para alocar mineradores nas dungeons vencidas.

Loja de Móveis: Gastar o Ouro ganho para colocar uma "Cama" no Grid que matematicamente recupera o HP.