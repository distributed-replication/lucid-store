------------------------------ MODULE Lucid ------------------------------
EXTENDS Naturals, FiniteSets

CONSTANT P,          \* The set of processes (2F + 1).
         F,          \* The maximum number of failures.
         Commands,   \* The set of state machine commands.
         LogSize,    \* The maximum size of the log.
         MaxClock    \* For the purpose of model checking

VARIABLES
  P_Clock,          \* P_Clock[p] is the value of p's logical clock.
  P_Slots,          \* P_Slot[p] is the log of each process.
  P_Index,          \* P_Index[p] is a pointer to the first empty slot.
  msgs              \* The set of messages sent by processes.
-----------------------------------------------------------------------------
Boolean == {TRUE, FALSE}

DRequest == [type : {"DRequest"}, C : Nat, D: Commands, S: P, I: Nat]
DConfirm == [type : {"DConfirm"}, C : Nat, D: Commands, S: P, I: Nat]
DCommit == [type : {"DCommit"}, D: Commands, I: Nat]

Messages == DRequest \cup DConfirm \cup DCommit

Slot == [D: Commands \cup {0}, C: Boolean]

Slots[i \in 1..LogSize] == Slot
(***************************************************************************)
(*                              THE INVARIANTS                             *)
(***************************************************************************)
LucidTypeOK ==
    /\ msgs \subseteq Messages
    /\ \A p \in P : /\ P_Clock[p] \in Nat
                    /\ \A s \in 1..LogSize : P_Slots[p][s] \in Slot
                    /\ P_Index[p] \in Nat
                    
LucidConsistent ==
    /\ \A p1, p2 \in P : /\ \A s \in 1..LogSize:
                                                \/ P_Slots[p1][s].D = P_Slots[p2][s].D
                                                \/ P_Slots[p1][s].C = FALSE
                                                \/ P_Slots[p2][s].C = FALSE
-----------------------------------------------------------------------------
(***************************************************************************)
(*                                THE PRIMITIVES                           *)
(***************************************************************************)
SlotAt(p, i) ==
    P_Slots[p][i]

SendMsg(m) ==
    msgs' = msgs \cup {m}

IncrementClock(p, clock) ==
    P_Clock' = [P_Clock EXCEPT ![p] = clock + 1]
    
IncrementIndex(p, index) ==
    P_Index' = [P_Index EXCEPT ![p] = index + 1]

SetSlot(p, D, C, I) ==
    P_Slots' = [P_Slots EXCEPT ![p] = [P_Slots[p] EXCEPT ![I] = [D |-> D, C |-> C]]]
(***************************************************************************)
(*                                THE ACTIONS                              *)
(***************************************************************************)
PassDecree(p) ==
    \/ \E i \in 1..LogSize:
        /\ i <= P_Index[p]
        /\ P_Index[p] <= LogSize
        /\ SlotAt(p, i).C = FALSE
        /\ P_Clock[p] < MaxClock
        /\ IncrementClock(p, P_Clock[p])
        /\ IF SlotAt(p, i).D = 0
            THEN \E c \in Commands:
                 LET msg == [type |-> "DRequest", C |-> P_Clock[p]', D |-> c, S |-> p, I |-> i]
                 IN /\ SetSlot(p, msg.D, FALSE, i)
                    /\ SendMsg(msg)
           ELSE LET msg == [type |-> "DRequest", C |-> P_Clock[p]', D |-> SlotAt(p, i).D, S |-> p, I |-> i]
                IN /\ SetSlot(p, msg.D, FALSE, i)
                   /\ SendMsg(msg)
        /\ UNCHANGED <<P_Index>>
        
ConfirmDRequest(p) ==
    /\ \E m \in msgs:
                    /\ m.type = "DRequest"
                    /\ m.C >= P_Clock[p]
                    /\ m.S # p
                    /\ IncrementClock(p, m.C)
                    /\ SlotAt(p, m.I).C = FALSE
                    /\ m.I <= LogSize
                    /\ IF SlotAt(p, m.I).D = 0
                        THEN LET msg == [type |-> "DConfirm", C |-> m.C, D |-> m.D, S |-> p, I |-> m.I]
                             IN /\ SetSlot(p, msg.D, FALSE, m.I)
                                /\ SendMsg(msg)
                       ELSE LET msg == [type |-> "DConfirm", C |-> m.C, D |-> SlotAt(p, m.I).D, S |-> p, I |-> m.I]
                            IN /\ SetSlot(p, msg.D, FALSE, m.I)
                               /\ SendMsg(msg)
                    /\ UNCHANGED <<P_Index>>

FinalizeDecree(p) ==
    /\ \E subset \in (SUBSET {m \in msgs : /\ m.type = "DConfirm"
                                           /\ m.C = P_Clock[p]
                                           /\ m.S # p
                                           /\ SlotAt(p, m.I).C = FALSE
                                           /\ m.D = SlotAt(p, m.I).D}):
        /\ \A m1, m2 \in subset : m1.I = m2.I
        /\ Cardinality({m.S : m \in subset}) = F
        /\ IncrementClock(p, P_Clock[p])
        /\ LET index == (CHOOSE m \in subset : TRUE).I
           IN /\ IncrementIndex(p, index)
              /\ SetSlot(p, SlotAt(p, index).D, TRUE, index)
              /\ SendMsg([type |-> "DCommit", D |-> SlotAt(p, index).D, I |-> index])

CommitDecree(p) ==
    /\ \E m \in msgs:
                        /\ m.type = "DCommit"
                        /\ SlotAt(p, m.I).C = FALSE
                        /\ SetSlot(p, m.D, TRUE, m.I)
                        /\ IncrementIndex(p, m.I)
                        /\ UNCHANGED <<P_Clock, msgs>>
                        
LucidInit ==
    /\ P_Clock = [p \in P |-> 0]
    /\ P_Index = [p \in P |-> 1]
    /\ P_Slots = [p \in P |-> [l \in 1..LogSize |-> [D |-> 0, C |-> FALSE]]]
    /\ msgs = {}
  
LucidNext ==
    \E p \in P : \/ PassDecree(p)
                 \/ ConfirmDRequest(p)
                 \/ FinalizeDecree(p)
                 \/ CommitDecree(p)
---------------------------------------------------------------------------------
vars == <<P_Clock, P_Slots, P_Index, msgs>>

LucidLiveness == \/ <>[](\A p \in P : \A i \in 1..LogSize : P_Slots[p][i].C = FALSE)
                 \/ <>[](\E p \in P : \E i \in 1..LogSize : P_Slots[p][i].C = TRUE)

LucidSpec ==
    /\ LucidInit
    /\ [][LucidNext]_vars
    \*/\ LucidLiveness

THEOREM LucidSpec => [](LucidTypeOK /\ LucidConsistent)

---------------------------------------------------------------------------------

=================================================================================
\* Modification History
