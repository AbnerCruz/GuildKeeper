
---

# 🏰 ProjectE - Guild Keeper

"**Guild Keeper**" é um simulador de gerenciamento de guilda medieval desenvolvido em "**MonoGame**". O jogador assume o papel de um mestre de guilda responsável por construir a base, recrutar heróis descartáveis e gerenciar o ciclo de vida de dungeons — que evoluem de ameaças mortais para minas de recursos lucrativas.

---

## 🎯 High Concept

Gerencie uma guilda onde heróis são ativos financeiros, e a exploração não termina com a morte do boss, mas sim com a exaustão total dos recursos naturais da dungeon.

## 💎 Pilares de Design

* **Heróis como Investimento:** O *permadeath* torna a perda de veteranos um impacto financeiro e estratégico real.
* **O Ciclo da Dungeon:** Combate (Risco)  Mineração (Recompensa Passiva)  Exaustão (Fim).
* **Estética Funcional:** A customização da base não é apenas visual; móveis e decoração concedem buffs passivos essenciais.

---

## ⚙️ Mecânicas Core

### A. O Hub (A Guilda)

* **Sistema:** Grid 2D (Tilemap) com slots para objetos.
* **Lógica de Buffs:**
* **Cama:** Recuperação de HP/Stress para heróis ociosos.
* **Mesa de Estratégia:** Bônus de XP em missões.
* **Decoração:** Mitigação passiva de stress.



### B. Os Aventureiros (Assets)

* **Atributos:** Força (Dano), Vitalidade (HP) e Velocidade (Tempo de missão).
* **Sistema de Stress:** Ao atingir 100%, o herói entra em *Burnout* (abandona a guilda ou para de trabalhar).
* **Traits (Traços):**
* 🟢 **Robusto:** Bônus de HP.
* 🟢 **Ganancioso:** Mais ouro no loot.
* 🔴 **Covarde:** Foge se o HP estiver abaixo de 20%.
* 🔴 **Bêbado:** Consome ouro da guilda periodicamente.



### C. As Dungeons (Loop Duplo)

1. **Fase 1: Ameaça (Ativa):** Requer heróis de combate. O objetivo é derrotar o Boss.
2. **Fase 2: Mina (Passiva):** Após a vitória, a dungeon torna-se uma mina. Requer alocação de mineradores para extração de ferro e pedra até que os recursos se esgotem (Idle).

---

## 🚀 Roteiro de Desenvolvimento (MVP)

### Fase 1: Lógica "Invisible" (Core Engine)

* [ ] Implementação das classes `Hero`, `Dungeon` e `ResourceBank`.
* [ ] Gerador procedural de atributos de Dungeons.
* [ ] Sistema de combate matemático (Dano vs. HP) sem interface.
* [ ] Lógica de conversão de estado: `Dungeon (HP <= 0) -> Mina`.

### Fase 2: Base Visual (MonoGame Basic)

* [ ] Renderização de Grid System (10x10).
* [ ] UI Base: Botões de Recrutamento e Construção usando `Rectangle`.
* [ ] Sprites de entidades e barras de status (HP/Stress) flutuantes.

### Fase 3: Loop de Gameplay

* [ ] Menu de Expedição (Seleção de Heróis -> Envio).
* [ ] Timer System para missões em tempo real.
* [ ] Sistema de alocação de mineradores.
* [ ] Loja de móveis com persistência de buffs no Grid.