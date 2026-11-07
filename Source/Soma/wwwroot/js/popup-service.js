let TIMEOUT;
const $listeners = new Map();
const $keylisteners = new Map();
const $scrolllisteners = new Map();
const ANIMATE_GAP = 100;

export function CalculatePosition(element, target, dotnet, options = {}) {
    clearTimeout(TIMEOUT);
    var popup = document.getElementById(target);
    if (!popup) return;

    const {
        placement = 'bottom',
        offset = 8,
        strategy = 'fixed',
        zIndex = 200,
        sync = false
    } = options;


    const {
        left: $parent_left,
        bottom: $parent_bottom,
        top: $parent_top,
        width: $parent_width
    } = element
            ? element.getBoundingClientRect()
            : { top: 0, bottom: 0, left: 0, right: 0, width: 0, height: 0 };

    Object.assign(popup.style, {
        display: 'block',
        visibility: 'hidden'
    });

    const {
        height: $target_height,
        width: $target_width
    } = popup.getBoundingClientRect();

    let x = 0;
    let y = 0;

    switch (placement) {
        case 'top':
            x = $parent_left;
            y = $parent_top - $target_height - offset;
            break;
        case 'bottom':
            x = $parent_left;
            y = $parent_bottom + offset;
            break;
        case 'left':
            x = $parent_left - $target_width - offset;
            y = $parent_top;
            break;
        case 'right':
            x = $parent_left + $parent_width + offset;
            y = $parent_top;
            break;
        default:
            x = $parent_left;
            y = $parent_bottom + offset;
            break;
    }

    if (placement === 'bottom' && (y + $target_height > window.innerHeight)) {
        y = $parent_top - $target_height - offset;
    } else if (placement === 'top' && (y < 0)) {
        y = $parent_bottom + offset;
    }

    if (x + (sync ? $parent_width : $target_width) > window.innerWidth) {
        x = window.innerWidth - (sync ? $parent_width : $target_width) - offset;
    }

    x = x < 0 ? offset : x;

    Object.assign(popup.style, {
        position: strategy,
        top: `${y}px`,
        left: `${x}px`,
        zIndex,
        transformOrigin: getTransformOrigin(placement),
        ...(sync && {
            width: `${$parent_width}px`,
            maxWidth: `${$parent_width}px`
        })
    });

    requestAnimationFrame(() => {
        popup.style.setProperty('visibility', 'visible', 'important');
        popup.style.setProperty('pointer-events', 'auto', 'important');
    });

    if (dotnet) {
        AddClickOutsideHandler(element, dotnet);
    }
}

function getTransformOrigin(placement) {
    const placements = {
        'top': 'bottom center',
        'bottom': 'top center',
        'left': 'center right',
        'right': 'center left',
    };

    return placements[placement] || 'center';
}

export function ClosePopup(id, target) {
    var popup = document.getElementById(target);
    if (!popup) return;

    RemoveListener(id);

    TIMEOUT = setTimeout(function () {
        const currentTop = parseInt(popup.style.top) || 0;
        Object.assign(popup.style, {
            visibility: 'hidden',
            top: `${currentTop + ANIMATE_GAP}px`,
            display: 'none',
        });
    }, 600);
}

export function AddClickOutsideHandler(element, dotnet = null) {
    if (!element) return;
    if (!dotnet) return;
    const { id } = element;
    RemoveListener(id);

    const handler = ({ target }) => {
        if (!element.contains(target)) {
            dotnet.invokeMethodAsync('JavascriptClose');
        }
    };
    const keyHandler = ({ key }) => {
        if (key === 'Escape') {
            dotnet.invokeMethodAsync('JavascriptClose');
        }
    };

    const scrollHandler = () => {
        dotnet.invokeMethodAsync('JavascriptClose');
    }

    $listeners.set(id, handler);
    $keylisteners.set(id, keyHandler);
    $scrolllisteners.set(id, scrollHandler);

    setTimeout(() => {
        document.addEventListener('click', handler);
        document.addEventListener('keyup', keyHandler);
        document.addEventListener('scroll', scrollHandler);
    }, 10);
}

export function RemoveListener(id) {
    const handler = $listeners.get(id);
    if (handler) {
        document.removeEventListener('click', handler);
        $listeners.delete(id);
    }
    const key_handler = $keylisteners.get(id);
    if (key_handler) {
        document.removeEventListener('keyup', key_handler);
        $keylisteners.delete(id);
    }
    const scroll_listener = $scrolllisteners.get(id);
    if (scroll_listener) {
        document.removeEventListener('scroll', scroll_listener);
        $scrolllisteners.delete(id);
    }
}