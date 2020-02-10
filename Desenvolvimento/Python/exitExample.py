import sys

while True:
    print('Digite sair para sair.')
    response = input()
    if response == 'sair':
        sys.exit()
    print('Você digitou ' + response + '.')
