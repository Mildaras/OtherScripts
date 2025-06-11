import numpy as np

# Parametrai:
dH298 = -114140      # Δ_rH°_298 [J/mol]
a     = -8.3         # Δ_r a [J/(mol·K)]
b     = 5.17e-3      # Δ_r b [J/(mol·K^2)]
d     = 9.09e5       # +Δ_r d (skaičiuojame d·(1/X - 1/298)) [J·K/mol]

def entalpija_X(X):
    term1 = dH298
    term2 = a * (X - 298)
    term3 = (b / 2) * (X**2 - 298**2)
    term4 = d * (1.0 / X - 1.0 / 298.0)
    return term1 + term2 + term3 + term4

# Temperatūrų sąrašas:
temperaturos = np.array([230, 240, 250, 260, 270, 280], dtype=float)

# Apskaičiuojame entalpijas kiekvienai temperatūrai (konvertuojame į Python float):
entalpijos = [float(entalpija_X(T)) for T in temperaturos]

# 1) Išvedame lentelę su Temperatūra ir entalpija:
print("Temperatūra (K) |   Δ_rH (J/mol)")
print("----------------+------------------")
for T, H in zip(temperaturos, entalpijos):
    print(f"   {int(T):6d}       {H:12.2f}")

# 2) Išvedame H ir T sąrašus kaip Python list literal:
formatted_H = "[" + ", ".join(f"{v:.4f}" for v in entalpijos) + "]"
formatted_T = "[" + ", ".join(f"{int(T)}" for T in temperaturos) + "]"

print("\nH list for copy-paste:")
print(formatted_H)
print("T list for copy-paste:")
print(formatted_T)
