import numpy as np

# Konstantos:
R1 = -8.3          # [J/(mol·K)] skaitiklis pirmajam terminui
R2 =  0.01157      # [J/(mol·K^2)] skaitiklis antram terminui
R3 = 909000        # [J·K/mol] skaitiklis trečiam terminui
R4 = 115231        # [J·K/mol] skaitiklis ketvirtam terminui
Rg = 8.314         # [J/(mol·K)]

# Funkcija ln(Kp) skaičiavimui:
def ln_Kp(T):
    term1 = (R1 / Rg) * np.log(T)
    term2 = (R2 / (2 * Rg)) * T
    term3 = - (R3 / (2 * Rg)) * (1.0 / T**2)
    term4 = - (R4 / Rg) * (1.0 / T)
    const = 60.9
    return term1 + term2 + term3 + term4 + const

# Temperatūrų sąrašas:
temperaturos = np.array([230, 240, 250, 260, 270, 280], dtype=float)

# Apskaičiuojame ln(Kp) kiekvienai temperatūrai:
lnKp_rezultatai = ln_Kp(temperaturos)

# 1) Išvedame lentelę su Temperatūra ir ln(Kp):
print("Temperatūra (K) |   ln(Kp)")
print("----------------+-----------")
for T, lnKp in zip(temperaturos, lnKp_rezultatai):
    print(f"   {T:7.1f}    | {lnKp:9.4f}")

# 2) Išvedame lnKp ir T sąrašus kaip Python list literal:
formatted_lnKp = "[" + ", ".join(f"{float(v):.4f}" for v in lnKp_rezultatai) + "]"
formatted_T    = "[" + ", ".join(f"{int(T)}" for T in temperaturos) + "]"

print("\nlnKp list for copy-paste:")
print(formatted_lnKp)
print("T list for copy-paste:")
print(formatted_T)
