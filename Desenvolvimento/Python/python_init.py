import sys
from math import cos, radians # Nessa forma da instrução import, as chamadas às funções em math não precisarão do prefixo math
import numpy as np     # installed with matplotlib
import matplotlib.pyplot as plt
import random


# Imprime uma mensagem

def hello_world():

	print("Hello, Visual Studio")

# Create a string with spaces proportional to a cosine of x in degrees

def math_cos():
	for i in range(360):
		print(cos(radians(i)))

def make_dot_string(x):
    rad = radians(x)                             # cos works with radians
    numspaces = int(20 * cos(rad) + 20)          # scale to 0-40 spaces
    st = ' ' * numspaces + 'o'                   # place 'o' after the spaces
    return st

def plot_graf():
    x = np.arange(0, radians(1800), radians(12))
    plt.plot(x, np.cos(x), 'b')
    plt.show()

def make_graf():
    for i in range(0, 1800, 12):
        s = make_dot_string(i)
        print(s)

# Estudo de entradas

def inputs():
	print()
	print('Qual o seu nome?') # pergunta o nome
	myName = input()
	print()
	print('Como é bom conhecer você, ' + myName)
	print('O tamanho do seu nome em caracteres é: ' + str(len(myName)))
	print()
	print('Qual a sua idade?') # pergunta a idade
	myAge = input()
	print()
	print('Você irá fazer ' + str(int(myAge) + 1) + ' em um ano.')
	print()

# Instruções if, elif e else

def vampire():
	print('Qual o seu nome?')
	myName = input()
	print()
	print('Qual a sua idade?')
	myAge = input()
	if myName == 'Fernando':
		print('Novamente, oi Fernando.')
	elif int(myAge) < 12:
		print('Você não é o Fernando, garoto')
	elif int(myAge) > 2000:
		print('Diferente de você, o Fernando não é um vampiro morto-vivo e imortal.')
	elif int(myAge) > 100:
		print('Você não é o Fernando, vovô.')
	else:
		print('Você não é o Fernando e nem uma criança.')
	print()

# Instruções de loop while

def cond_if():
	print('Usando a instrução if:')
	print()
	spam = 0
	if spam < 5:
		print('Hello, world.')
		spam = spam + 1
	print()

def cond_while():
	print('Usando a instrução while:')
	print()
	spam = 0
	while spam < 5:
		print('Hello, world.')
		spam = spam + 1
	print()

def your_name():
	name = ''
	while name != 'Fernando':
		print('Por favor, digite o seu nome:')
		name = input()
	print('Obrigado, ' + name + '!')

# Instruções break

def your_name_break():
	while True:
		print('Por favor, digite o seu nome:')
		name = input()
		if name == 'Fernando':
			break
	print('Obrigado, ' + name + '!')

# Em caso de um bug com loop, tecle "CTRL + C" (envia um KeyboardInterrupt)

def infinite_loop():
	while True:
		print('Hello world!')

# Instruções continue

def swordfish():
	while True:
		print('Que é você?')
		name = input()
		if name != 'Fernando':
			continue
		print('Olá, ' + name + '. Qual a senha? (Dica: é um peixe!)')
		password = input()
		if password == 'swordfish':
			break
	print('Acesso liberado!')

# VALORES “TRUTHY” E “FALSEY” 
# Há alguns valores de outros tipos de dados para os quais as condições os
# considerarão equivalentes a True e False. Quando usados em condições, 0, 0.0 e '' (a string vazia) 
# são considerados False, enquanto todos os demais valores são considerados True.

def truthy_falsey():
	name = ''
	while not name:
		print('Digite o seu nome:')
		name = input()
	print('Quantos convidados espera receber?')
	numOfGuests = int(input())
	if numOfGuests:
		print('Certifique-se de ter espaço suficiente para todos os seus convidados.')
	print('Feito')

# Loops for e a função range()

def  fiveTimes_for():
	print('Meu nome é:')
	for i in range(5):
		print('Fernando Five Times (' + str(i) + ')')

# Karl Friedrich Gauss, quando ainda era criança, um professor professor disse aos alunos 
# para que somassem todos os números de 0 a 100. O jovem Gauss concebeu um truque inteligente 
# para descobrir a resposta em alguns segundos, porém você pode criar um programa Python com 
# um loop for para fazer esse cálculo.
# (O jovem Gauss descobriu que havia 50 pares de números que somavam
# 100: 1 + 99, 2 + 98, 3 + 97 e assim por diante, até 49 + 51. Como 50 × 100 é
# igual a 5.000, ao somar aquele 50 intermediário, a soma de todos os números
# de 0 a 100 será 5.050. Garoto esperto!)


def gauss():
	total = 0
	for num in range(101):
		total = total + num
	print(total)

def fiveTimes_while():
	print('Menu nome é:')
	i = 0
	while i < 5:
		print('Fernando Five Times (' + str(i) + ')')
		i = i + 1

# Argumentos de início, fim e de incremento de range()

def args_range():
	for i in range(12, 16):
		print(i)

def args_range_increment():
	for i in range(0, 10, 2):
		print(i)

def args_range_negative():
	for i in range(5, -1, -1):
		print(i)

# Importando módulos

# Os programas Python podem chamar um conjunto básico de funções denominado funções internas (built-in), 
# incluindo as funções print(), input() e len(), que vimos anteriormente. O Python também vem com um 
# conjunto de módulos chamado biblioteca-padrão (standard library). Para usá-los faça por uma instrução import

def printRandom():
	for i in range(5):
		print(random.randint(1, 10))

def exitExample():
	while True:
		print('Digite sair para sair.')
		response = input()
		if response == 'sair':
			sys.exit()
		print('Você digitou ' + response + '.')

def main():
	#hello_world()
	#math_cos()
	#inputs()
	#make_graf()
	#plot_graf()
	#vampire()
	#cond_if()
	#cond_while()
	#your_name()
	#your_name_break()
	#infinite_loop()
	#swordfish()
	#truthy_falsey()
	#fiveTimes_for()
	#gauss()
	#fiveTimes_while()
	#args_range()
	#args_range_increment()
	#args_range_negative()
	#printRandom()
	exitExample()

main()
