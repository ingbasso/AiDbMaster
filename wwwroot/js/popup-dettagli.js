/**
 * ========================================
 * POPUP DETTAGLI - JavaScript Riutilizzabile
 * ========================================
 * 
 * Funzioni per gestire popup trascinabili con la grafica standard
 * 
 * UTILIZZO:
 * 1. Includi questo file nella tua pagina:
 *    <script src="~/js/popup-dettagli.js"></script>
 * 
 * 2. Inizializza il popup:
 *    const myPopup = new PopupDettagli('myPopupId', 'myPopupHeaderId');
 * 
 * 3. Usa i metodi:
 *    myPopup.show();
 *    myPopup.hide();
 *    myPopup.setContent(htmlContent);
 */

class PopupDettagli {
    /**
     * Costruttore
     * @param {string} popupId - ID del div popup (senza #)
     * @param {string} headerId - ID dell'header popup (senza #)
     */
    constructor(popupId, headerId) {
        this.popup = document.getElementById(popupId);
        this.header = document.getElementById(headerId);
        this.body = this.popup.querySelector('.popup-body');
        this.footer = this.popup.querySelector('.popup-footer');
        
        if (!this.popup || !this.header) {
            console.error(`PopupDettagli: Elementi non trovati (popup: ${popupId}, header: ${headerId})`);
            return;
        }
        
        // Rendi il popup trascinabile
        this.makeDraggable();
    }
    
    /**
     * Mostra il popup centrato nella posizione standard (10% dall'alto)
     */
    show() {
        if (!this.popup) return;
        
        this.popup.style.display = 'block';
        this.popup.style.left = '50%';
        this.popup.style.top = '10%';
        this.popup.style.transform = 'translateX(-50%)';
    }
    
    /**
     * Nascondi il popup
     */
    hide() {
        if (!this.popup) return;
        this.popup.style.display = 'none';
    }
    
    /**
     * Imposta il contenuto del body
     * @param {string} htmlContent - HTML da inserire nel body
     */
    setContent(htmlContent) {
        if (!this.body) return;
        this.body.innerHTML = htmlContent;
    }
    
    /**
     * Mostra/nascondi il footer
     * @param {boolean} show - true per mostrare, false per nascondere
     */
    toggleFooter(show) {
        if (!this.footer) return;
        this.footer.style.display = show ? 'flex' : 'none';
    }
    
    /**
     * Mostra un alert nel popup
     * @param {string} message - Messaggio da mostrare
     * @param {string} type - Tipo di alert ('warning', 'success', 'danger')
     */
    showAlert(message, type = 'warning') {
        let alertDiv = this.body.querySelector('.alert-fermo-centro');
        
        if (!alertDiv) {
            // Crea alert se non esiste
            alertDiv = document.createElement('div');
            alertDiv.className = 'alert-fermo-centro';
            this.body.appendChild(alertDiv);
        }
        
        // Colori basati sul tipo
        const colors = {
            warning: { bg: '#fff3cd', border: '#ffc107', text: '#856404' },
            success: { bg: '#d4edda', border: '#28a745', text: '#155724' },
            danger: { bg: '#f8d7da', border: '#dc3545', text: '#721c24' }
        };
        
        const color = colors[type] || colors.warning;
        
        alertDiv.style.background = color.bg;
        alertDiv.style.borderColor = color.border;
        alertDiv.style.color = color.text;
        alertDiv.innerHTML = message;
        alertDiv.classList.add('visible');
    }
    
    /**
     * Nascondi l'alert
     */
    hideAlert() {
        const alertDiv = this.body.querySelector('.alert-fermo-centro');
        if (alertDiv) {
            alertDiv.classList.remove('visible');
        }
    }
    
    /**
     * Rende il popup trascinabile dall'header
     * @private
     */
    makeDraggable() {
        let pos1 = 0, pos2 = 0, pos3 = 0, pos4 = 0;
        const popup = this.popup;
        
        this.header.onmousedown = dragMouseDown;

        function dragMouseDown(e) {
            e = e || window.event;
            e.preventDefault();
            pos3 = e.clientX;
            pos4 = e.clientY;
            document.onmouseup = closeDragElement;
            document.onmousemove = elementDrag;
            popup.classList.add('dragging');
        }

        function elementDrag(e) {
            e = e || window.event;
            e.preventDefault();
            pos1 = pos3 - e.clientX;
            pos2 = pos4 - e.clientY;
            pos3 = e.clientX;
            pos4 = e.clientY;
            popup.style.top = (popup.offsetTop - pos2) + 'px';
            popup.style.left = (popup.offsetLeft - pos1) + 'px';
            popup.style.transform = 'none';
        }

        function closeDragElement() {
            document.onmouseup = null;
            document.onmousemove = null;
            popup.classList.remove('dragging');
        }
    }
    
    /**
     * Genera HTML per un campo info standard
     * @param {string} label - Etichetta campo
     * @param {string} value - Valore campo
     * @param {string} spanClass - Classe opzionale (span-2, span-3, full-width)
     * @param {string} valueClass - Classe valore opzionale (highlight, calculated)
     * @returns {string} HTML del campo
     */
    static createInfoField(label, value, spanClass = '', valueClass = '') {
        return `
            <div class="info-item ${spanClass}">
                <div class="info-label">${label}</div>
                <div class="info-value ${valueClass}">${value}</div>
            </div>
        `;
    }
    
    /**
     * Genera HTML per un campo input modificabile
     * @param {string} label - Etichetta campo
     * @param {string} id - ID dell'input
     * @param {string} value - Valore iniziale
     * @param {string} type - Tipo input (text, number, datetime-local, etc.)
     * @param {boolean} disabled - Se l'input è disabilitato
     * @param {string} spanClass - Classe opzionale (span-2, span-3, full-width)
     * @returns {string} HTML del campo input
     */
    static createInputField(label, id, value, type = 'text', disabled = false, spanClass = '') {
        return `
            <div class="info-item ${spanClass}">
                <div class="info-label">${label}</div>
                <input 
                    type="${type}" 
                    id="${id}" 
                    class="info-input" 
                    value="${value}"
                    ${disabled ? 'disabled' : ''}
                />
            </div>
        `;
    }
    
    /**
     * Genera HTML per una griglia completa di campi
     * @param {Array<Object>} fields - Array di oggetti {label, value, spanClass, valueClass}
     * @returns {string} HTML della griglia
     */
    static createInfoGrid(fields) {
        let html = '<div class="info-grid">';
        
        fields.forEach(field => {
            html += PopupDettagli.createInfoField(
                field.label, 
                field.value, 
                field.spanClass || '', 
                field.valueClass || ''
            );
        });
        
        html += '</div>';
        return html;
    }
}

/**
 * ESEMPIO DI UTILIZZO:
 * 
 * // 1. Inizializza il popup
 * const myPopup = new PopupDettagli('myPopupId', 'myPopupHeaderId');
 * 
 * // 2. Crea contenuto con helper
 * const fields = [
 *     { label: 'Nome', value: 'Mario Rossi', spanClass: 'span-2' },
 *     { label: 'Quantità', value: '150', valueClass: 'highlight' },
 *     { label: 'Totale', value: '1250.00 €', valueClass: 'calculated' }
 * ];
 * 
 * const content = PopupDettagli.createInfoGrid(fields);
 * 
 * // 3. Mostra il popup
 * myPopup.setContent(content);
 * myPopup.show();
 * 
 * // 4. Mostra un alert
 * myPopup.showAlert('⚠️ Attenzione: verifica i dati', 'warning');
 * 
 * // 5. Chiudi il popup
 * myPopup.hide();
 */

