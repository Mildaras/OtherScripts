import numpy as np

# Pradinės ln(Kp) reikšmės:
lnKp_list = np.array([-5.6626, -3.1030, -0.7524, 1.4137, 3.4161, 5.2727], dtype=float)
T_list     = np.array([230,    240,    250,    260,    270,    280   ], dtype=float)

# Apskaičiuojame Kp = exp(ln Kp) kiekvienai reikšmei:
Kp_list = np.exp(lnKp_list)
# Konvertuojame į paprastus Python float:
Kp_list = [float(x) for x in Kp_list]

# 1) Išvedame lentelę su Temperatūra ir Kp:
print("Temperatūra (K) |    Kp")
print("----------------+----------")
for T, Kp in zip(T_list, Kp_list):
    print(f"   {T:7.1f}    | {Kp:8.4f}")

# 2) Išvedame Kp ir T sąrašus kaip Python list literal:
formatted_Kp = "[" + ", ".join(f"{v:.4f}" for v in Kp_list) + "]"
formatted_T  = "[" + ", ".join(f"{int(T)}" for T in T_list) + "]"

print("\nKp list for copy-paste:")
print(formatted_Kp)
print("T list for copy-paste:")
print(formatted_T)
