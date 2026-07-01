from PySide6.QtWidgets import (
    QWidget,
    QLabel,
    QPushButton,
    QVBoxLayout,
    QListWidget
)

class MainWindow(QWidget):

    def __init__(self):
        super().__init__()

        self.setWindowTitle("Kaching Windows")
        self.resize(450, 550)

        layout = QVBoxLayout()

        self.status = QLabel("🟢 Estado: Detenido")
        self.sales = QLabel("Ventas hoy: 0")
        self.total = QLabel("Total vendido: 0 €")

        layout.addWidget(self.status)
        layout.addWidget(self.sales)
        layout.addWidget(self.total)

        layout.addWidget(QLabel("Últimas ventas"))

        self.history = QListWidget()

        layout.addWidget(self.history)

        self.start = QPushButton("▶ Iniciar")

        self.stop = QPushButton("■ Detener")

        self.test = QPushButton("🔊 Probar sonido")

        layout.addWidget(self.start)
        layout.addWidget(self.stop)
        layout.addWidget(self.test)

        self.setLayout(layout)
